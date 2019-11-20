
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

    [FormerlySerializedAs("SMPLMesh_Male")]
    [FormerlySerializedAs("smpl_m")]
    public Mesh SMPLMesh_Male_Prefab;
     
    [FormerlySerializedAs("SMPLMesh_Female")]
    [FormerlySerializedAs("smpl_f")]
    public Mesh SMPLMesh_Female_Prefab;
    
    
    

    public bool ChangeFrameRate = false;
    
    [FormerlySerializedAs("FrameRate")]
    public int DesiredFrameRate;

    public SkinnedMeshRenderer meshRenderer;

    public BMLModifyBones boneModifier;

    int currentFrame = 0;
    Quaternion[] poses;

    Mesh smplMeshClone;

    /// <summary>
    /// Has the current animation finished playing, if one has been loaded.
    /// </summary>
    public bool AnimDone => currentFrame >= moshAnimation.GetResampledTotalFrameCount;
    
    void Awake()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();

        poses = new Quaternion[SMPLConstants.JointCount];
        
        //Rotate the FBX model in case up direction is not Y - axis;
        // this seems weird. 
        if (SMPLConstants.ZAxisUp) {
            transform.parent.Rotate(-90f, 0f, 0f);
        }

        boneModifier = new BMLModifyBones(meshRenderer);
        
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
        // 4. Set Betas of avg FBX model in the scene to betas from Mosh file
        float[] blend = new float[SMPLConstants.DoubledShapeBlendCount];
        moshAnimation.GetShapeBlendValues(blend);
        SetBetaBlshapeValues(blend);
        // 5. Calculate INITIAL joint-locations from betas & update joints of the FBX model
        CalculateJoints();

    
    }

    // should be added. Passing a text file into this isn't a great design.     
    //public void PlayAnim(MoshAnimation moshanim) {
    //    throw new NotImplementedException("");
    //}


    void Update() {
        PlayCurrentFrame();
    }


    void PlayCurrentFrame() {
        
        if (!AnimDone) {
            Vector3 t = moshAnimation.GetTranslationAtFrame(currentFrame);
            moshAnimation.GetPose(poses, currentFrame);
            boneModifier.updateBoneAngles(poses, t);
            SetPoseBlendValues();
            currentFrame++;
        }
        
    }

    void SetPoseBlendValues()
    {
        // start at 1 to skip pelvis. 
        // pelvis has a rotation, but doesn't seem to have associated blend shapes.
        for (int poseIndex = 1; poseIndex < poses.Length; poseIndex++) {
            // i is equivalent to index for the other version. 
            Quaternion currentPose = poses[poseIndex];
            float[] rot3x3 = MoShUtilities.Quat_to_3x3Mat(currentPose);
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
                meshRenderer.SetBlendShapeWeight(SMPLConstants.DoubledShapeBlendCount + doubledIndex + rotationMatrixElementIndex + 0, pos * 100.0f);
                meshRenderer.SetBlendShapeWeight(SMPLConstants.DoubledShapeBlendCount + doubledIndex + rotationMatrixElementIndex + 1, neg * 100.0f);
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
        boneModifier.updateBonePositions(joints, true);
    }

    /// <summary>
    /// Set the values of the first 20 blendshapes in the skinned 
    /// mesh renderer, defining body shape. 
    /// </summary>
    /// <param name="bweights">Values assigned to blendshapes.</param>
    void SetBetaBlshapeValues(float[] bweights) 
    {
        //!!!! float beta = betas[i] / SCALE; <- this was in original. It's important!!!
        Debug.Assert(bweights.Length == SMPLConstants.DoubledShapeBlendCount, "bweights array too small.");
        for (int shapeIndex = 0; shapeIndex < SMPLConstants.DoubledShapeBlendCount; shapeIndex++) {
            meshRenderer.SetBlendShapeWeight(shapeIndex, bweights[shapeIndex]);
        }
    }


    void ResetBlendShapes() {
        for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++) {
            meshRenderer.SetBlendShapeWeight(i, 0f);
        }
    }


    /// <summary>
    /// Called by PlayAnim to reset the skeleton before playing another animation.
    /// </summary>
    public void Reset()
    {
        boneModifier.ResetRotations();
        ResetBlendShapes();
        Vector3[] joints = JointCalculator.GetDefaultJoints(moshAnimation.Gender);
        boneModifier.updateBonePositions(joints, true);
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
                smplMeshClone = Instantiate(SMPLMesh_Female_Prefab);
                break;
            }
            case Gender.MALE:
                smplMeshClone = Instantiate(SMPLMesh_Male_Prefab);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
        }
        
        meshRenderer.sharedMesh = smplMeshClone;
    }
    

}