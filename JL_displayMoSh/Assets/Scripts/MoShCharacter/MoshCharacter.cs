
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
    Mesh smplMeshClone;

   

    public bool ChangeFrameRate = false;
    
    [FormerlySerializedAs("FrameRate")]
    public int DesiredFrameRate;

    [FormerlySerializedAs("meshRenderer")]
    public SkinnedMeshRenderer MeshRenderer;

    [FormerlySerializedAs("boneModifier")]
    public BMLModifyBones BoneModifier;

    int currentFrame = 0;
    
    [SerializeField]
    SMPLSettings Settings = default;


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
    public void StartAnimation(String jsonAnimationFileWholeString)
    {
        transform.parent.gameObject.SetActive(true);
        
        if (moshAnimation != null) {
            Reset();
        }
        
        moshAnimation = new MoShAnimationFromJSON(jsonAnimationFileWholeString).Build();
        moshAnimation.AttachAnimationToMoshCharacter(this);
        
        currentFrame = 0;

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
        else {
            if (Settings.HideMeshWhenFinished) transform.parent.gameObject.SetActive(false);
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

    
    public void ActivateMesh(Gender gender) {
        MeshRenderer.sharedMesh = Instantiate(Settings.GetMeshPrefab(gender));
    }
    
}