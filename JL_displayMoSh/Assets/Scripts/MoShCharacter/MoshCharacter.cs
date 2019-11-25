
using UnityEngine;
using System;
using UnityEngine.Serialization;

/// <summary>
/// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
/// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
/// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPL model.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class MoshCharacter : MonoBehaviour {

    MoshAnimation moshAnimation;

    // A bunch of the following fields can basically be considered internal to the program and should be 
    // read in, rather than entered manually in the editor. 

    [FormerlySerializedAs("SMPLMesh_Male_Prefab")]
    [FormerlySerializedAs("SMPLMesh_Male")]
    [FormerlySerializedAs("smpl_m")]
    public Mesh SMPLMeshMalePrefab;
     
    [FormerlySerializedAs("SMPLMesh_Female_Prefab")]
    [FormerlySerializedAs("SMPLMesh_Female")]
    [FormerlySerializedAs("smpl_f")]
    public Mesh SMPLMeshFemalePrefab;
    
    
    

    public bool ChangeFrameRate = false;
    
    [FormerlySerializedAs("FrameRate")]
    public int DesiredFrameRate;

    [FormerlySerializedAs("meshRenderer")]
    public SkinnedMeshRenderer MeshRenderer;

    [FormerlySerializedAs("boneModifier")]
    public BMLModifyBones BoneModifier;

    int currentFrame = 0;
    
    Mesh smplMeshClone;

    /// <summary>
    /// Has the current animation finished playing, if one has been loaded.
    /// </summary>
    public bool AnimDone => currentFrame >= moshAnimation.GetResampledTotalFrameCount;
    
    void Awake()
    {
        MeshRenderer = GetComponent<SkinnedMeshRenderer>();
        
        //Rotate the FBX model in case up direction is not Y - axis;
        // this seems weird. 
        if (SMPL.ZAxisUp) {
            transform.parent.Rotate(-90f, 0f, 0f);
        }

        BoneModifier = new BMLModifyBones(MeshRenderer);
        
    }


    /// <summary>
    /// Sets up and plays a mosh animation.
    /// This is the main point of interaction with the load MoSh functionality.
    /// Give it a file, call the method and it will do the whole thing.
    /// </summary>
    public void PlayAnim(TextAsset jsonAnimationFile)
    {
        if (moshAnimation != null) {
            Reset();
        }
        moshAnimation = new MoShAnimationFromJSON(jsonAnimationFile).Build();

        ActivateMesh(moshAnimation.Gender);

        if (ChangeFrameRate && DesiredFrameRate != 0) {
            moshAnimation.SetDesiredFPS(DesiredFrameRate);
        }

        currentFrame = 0;
        // 4. Set Betas of avg FBX model in the scene to shapeBetas from Mosh file
        SetMeshShapeBetas(moshAnimation.GetDoubledBetas());
        
        // 5. Calculate INITIAL joint-locations from shapeBetas & update joints of the FBX model
        CalculateJoints();

    
    }
    

    void Update() {
        PlayCurrentFrame();
    }


    void PlayCurrentFrame() {
        
        if (!AnimDone) {
            Vector3 t = moshAnimation.GetTranslationAtFrame(currentFrame);
            Quaternion[] poses = moshAnimation.GetPoseAtFrame(currentFrame);
            BoneModifier.UpdateBoneAngles(poses, t);
            SetPoseAsCurrentFrame(poses);
            currentFrame++;
        }
        
    }

    void SetPoseAsCurrentFrame( Quaternion[] poses) {
        // start at 1 to skip pelvis. 
        // pelvis has a rotation, but doesn't seem to have associated blend shapes.
        for (int poseIndex = 1; poseIndex < poses.Length; poseIndex++) {
            // i is equivalent to index for the other version. 
            Quaternion currentPose = poses[poseIndex];
            float[] rot3x3 = MoShUtilities.QuaternionTo3X3Matrix(currentPose);
            int index = (poseIndex - 1) * 9;
            for (int rotationMatrixElementIndex = 0; rotationMatrixElementIndex < 9; rotationMatrixElementIndex++) {
                float pos, neg;
                float theta = rot3x3[rotationMatrixElementIndex];
                if (theta >= 0) {
                    pos = theta;
                    neg = 0f;
                } else {
                    pos = 0.0f;
                    neg = -theta;
                }

                int doubledIndex = index * 2;
                MeshRenderer.SetBlendShapeWeight(SMPL.DoubledShapeBetaCount + doubledIndex + rotationMatrixElementIndex + 0, pos * 100.0f);
                MeshRenderer.SetBlendShapeWeight(SMPL.DoubledShapeBetaCount + doubledIndex + rotationMatrixElementIndex + 1, neg * 100.0f);
            }
        }
    }


    /// <summary>
    /// Gets the new joint positions from the animation.
    /// Passes them to the boneModifier. 
    /// </summary>
    void CalculateJoints()
    {
        Vector3[] joints = moshAnimation.GetJoints();
        BoneModifier.UpdateBonePositions(joints, true);
    }

    /// <summary>
    /// Set the values of the first 20 blendshapes in the skinned 
    /// mesh renderer, defining body shape. 
    /// </summary>
    /// <param name="shapeBetas">Values assigned to blendshapes.</param>
    void SetMeshShapeBetas(float[] shapeBetas) 
    {
        //!!!! float beta = shapeBetas[i] / SCALE; <- this was in original. It's important!!!
        for (int betaIndex = 0; betaIndex < SMPL.DoubledShapeBetaCount; betaIndex++) {
            MeshRenderer.SetBlendShapeWeight(betaIndex, shapeBetas[betaIndex]);
        }
    }


    void ResetBlendShapes() {
        for (int blendShapeIndex = 0; blendShapeIndex < MeshRenderer.sharedMesh.blendShapeCount; blendShapeIndex++) {
            MeshRenderer.SetBlendShapeWeight(blendShapeIndex, 0f);
        }
    }


    /// <summary>
    /// Called by PlayAnim to reset the skeleton before playing another animation.
    /// </summary>
    public void Reset()
    {
        BoneModifier.ResetRotations();
        ResetBlendShapes();
        Vector3[] joints = JointCalculator.GetDefaultJoints(moshAnimation.Gender);
        BoneModifier.UpdateBonePositions(joints, true);
    }


    /// <summary>
    /// Activate the mesh for the specific gender and assigns it to the 
    /// skinned mesh renderer. 
    /// The mesh is first cloned if this has not previously been done.
    /// </summary>
    /// <param name="gender">Gender of mesh to swap in</param>
    void ActivateMesh(Gender gender) {
        switch (gender) {
            case Gender.Female: {
                smplMeshClone = Instantiate(SMPLMeshFemalePrefab);
                break;
            }
            case Gender.MALE:
                smplMeshClone = Instantiate(SMPLMeshMalePrefab);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
        }
        
        MeshRenderer.sharedMesh = smplMeshClone;
    }
    

}