
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

    [FormerlySerializedAs("smpl_m")]
    public Mesh SMPLMesh_Male; 
    
    [FormerlySerializedAs("smpl_f")]
    public Mesh SMPLMesh_Female;
    
    
    // cloned flags are set when the mesh has been cloned. 
    bool meshCloned_m = false;
    bool meshCloned_f = false;
    

    public bool ChangeFrameRate = false;
    public int FrameRate;

    public SkinnedMeshRenderer meshRenderer;

    public BMLModifyBones boneModifier;

    public int moshFrame = 0;
    Quaternion[] pose;

    bool animLoaded = false;


    /// <summary>
    /// Has the current animation finished playing, if one has been loaded.
    /// </summary>
    public bool AnimDone => animLoaded && moshFrame >= moshAnimation.GetResampledTotalFrameCount;
    
    void Awake()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>();

        pose = new Quaternion[SMPLConstants.JointCount];
        
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

        if (ChangeFrameRate && FrameRate != 0) {
            moshAnimation.SetDesiredFPS(FrameRate);
        }

        moshFrame = 0;
        // 4. Set Betas of avg FBX model in the scene to betas from Mosh file
        float[] blend = new float[SMPLConstants.DoubledShapeBlendCount];
        moshAnimation.GetShapeBlendValues(blend);
        SetBetaBlshapeValues(blend);
        // 5. Calculate INITIAL joint-locations from betas & update joints of the FBX model
        CalculateJoints();
        animLoaded = true;

    
    }

    // should be added. Passing a text file into this isn't a great design.     
    //public void PlayAnim(MoshAnimation moshanim) {
    //    throw new NotImplementedException("");
    //}


    void Update() {
        PlayCurrentFrame();
    }


    public void PlayCurrentFrame() {
        if (animLoaded) {
            if (!AnimDone) {
                Vector3 t = moshAnimation.GetTranslationAtFrame(moshFrame);
                moshAnimation.GetPose(pose, moshFrame);
                boneModifier.updateBoneAngles(pose, t);
                SetPoseBlendValues();
                moshFrame++;
            }
        }
    }

    void SetPoseBlendValues()
    {
        // start at 1 to skip pelvis. 
        // pelvis has a rotation, but doesn't seem to have associated blend shapes.
        for (int i = 1; i < pose.Length; i++) {
            // i is equivalent to index for the other version. 
            Quaternion qi = pose[i];
            float[] rot3x3 = MoShUtilities.Quat_to_3x3Mat(qi);
            int idx = (i - 1) * 9;
            for (int elem = 0; elem < 9; elem++) {
                float pos, neg;
                float theta = rot3x3[elem];
                if (theta >= 0) {
                    pos = theta;
                    neg = 0f;
                } else {
                    pos = 0.0f;
                    neg = -theta;
                }
                meshRenderer.SetBlendShapeWeight(SMPLConstants.DoubledShapeBlendCount + (idx * 2) + elem + 0, pos * 100.0f);
                meshRenderer.SetBlendShapeWeight(SMPLConstants.DoubledShapeBlendCount + (idx * 2) + elem + 1, neg * 100.0f);
            }
        }
    }


    /// <summary>
    /// Gets the new joint positions from the animation.
    /// Passes them to the boneModifier. 
    /// </summary>
    public void CalculateJoints()
    {
        Vector3[] joints = moshAnimation.GetJoints();
        boneModifier.updateBonePositions(joints, true);
    }

    /// <summary>
    /// Set the values of the first 20 blendshapes in the skinned 
    /// mesh renderer, defining body shape. 
    /// </summary>
    /// <param name="bweights">Values assigned to blendshapes.</param>
    public void SetBetaBlshapeValues(float[] bweights) 
    {
        //!!!! float beta = betas[i] / SCALE; <- this was in original. It's important!!!
        Debug.Assert(bweights.Length == 20, "bweights array too small.");
        for (int i = 0; i < 20; i++) {
            meshRenderer.SetBlendShapeWeight(i, bweights[i]);
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
        //boneModifier.reset();
        //resetJoints();
        animLoaded = false;
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
                if (!meshCloned_f) {
                    SMPLMesh_Female = Instantiate(SMPLMesh_Female);
                    meshCloned_f = true;
                }
                // don't want to do this if using editor functionality, because the 
                // variable meshRenderer will not have been initialized. 
                if (Application.isPlaying) {
                    //Debug.Log("is playing. not editor code.");
                    meshRenderer.sharedMesh = SMPLMesh_Female;
                }

                break;
            }
            case Gender.MALE:
                if (!meshCloned_m) {
                    SMPLMesh_Male = Instantiate(SMPLMesh_Male);
                    meshCloned_m = true;
                }
                if (Application.isPlaying) {
                    meshRenderer.sharedMesh = SMPLMesh_Male;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
        }
    }
    

}