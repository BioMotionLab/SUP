
using UnityEngine;
using System;

/// <summary>
/// Altered version of loadMoshAnim from MPI. Allows a MoSh animation to be played at any time with a call to PlayAnim,
/// rather than running on initialization of the script in the start method (requiring instantiating a new prefab.
/// This is setup to handle loading a MoSh animation without prior knowledge of gender, swapping in the correct SMPL model.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public partial class MoShCharacter : MonoBehaviour {

    MoShAnimation anim;
    public MoShAnimation CurrentAnim {
        get {
            return anim;
        }
    }
    string animFilename;

    // A bunch of the following fields can basically be considered internal to the program and should be 
    // read in, rather than entered manually in the editor. 

    // cloned flags are set when the mesh has been cloned. 
    public Mesh smpl_m; //references to the mesh assets.
    bool meshCloned_m = false;
    public Mesh smpl_f;
    bool meshCloned_f = false;

    // not used.
    //private Mesh OGMesh_m;
    //private Mesh OGMesh_f;

    //public char upAxis = 'z';
    char upAxis = 'z';

    public bool ChangeFrameRate = false;
    public int FrameRate;

    private SkinnedMeshRenderer meshRenderer;

    private BMLModifyBones boneModifier;

    private int moshFrame = 0;
    private Quaternion[] pose;

    private bool animLoaded = false;


    /// <summary>
    /// Whether an animation has been loaded or not.
    /// </summary>
    public bool AnimLoaded {
        get { 
            return animLoaded; 
        }
    }


    /// <summary>
    /// Has the current animation finished playing, if one has been loaded.
    /// </summary>
    public bool AnimDone {
        get { 
            return animLoaded && moshFrame >= anim.GetResampledTotalFrameCount; 
        }
    }


    void Awake()
    {
        pose = new Quaternion[MoShAnimation.JointCount];
        Debug.Assert(smpl_m != null, "male SMPL mesh not provided. Please provide the mesh in the inspector.");
        Debug.Assert(smpl_f != null, "female SMPL mesh not provided. Please provide the mesh in the inspector.");

        meshRenderer = GetComponent<SkinnedMeshRenderer>();

        //Rotate the FBX model in case up direction is not Y - axis;
        // this seems weird. 
        if (upAxis == 'z') {
            transform.parent.Rotate(-90f, 0f, 0f);
        }

        boneModifier = new BMLModifyBones(meshRenderer);

        InitializeLogging();
    }


    /// <summary>
    /// Sets up and plays a mosh animation.
    /// This is the main point of interaction with the load MoSh functionality.
    /// Give it a file, call the method and it will do the whole thing.
    /// </summary>
    /// <remarks>

    /// </remarks>
    public void PlayAnim(TextAsset animfile)
    {
        if (anim != null) {
            reset();
        }
        anim = new MoShAnimationJSON(animfile);
        animFilename = animfile.name;

        activateMesh(anim.GetGender);

        if (ChangeFrameRate && FrameRate != 0) {
            anim.SetDesiredFPS(FrameRate);
        }

        moshFrame = 0;
        // 4. Set Betas of avg FBX model in the scene to betas from Mosh file
        float[] blend = new float[20];
        anim.GetShapeBlendValues(blend);
        setBetaBlshapeValues(blend);
        // 5. Calculate INITIAL joint-locations from betas & update joints of the FBX model
        calculateJoints();
        animLoaded = true;

        // doesn't matter if logging is enabled or disabled. It checks for that 
        // in the function.
        StartLogs();
    }

    // should be added. Passing a text file into this isn't a great design.     
    //public void PlayAnim(MoShAnimation moshanim) {
    //    throw new NotImplementedException("");
    //}


    void Update() {
        PlayCurrentFrame();
    }


    public void PlayCurrentFrame() {
        if (animLoaded) {
            if (!AnimDone) {
                Vector3 t = anim.GetTranslationAtFrame(moshFrame);
                anim.GetPose(pose, moshFrame);
                boneModifier.updateBoneAngles(pose, t);
                setPoseBlendValues();
                moshFrame++;
                LogLine();
            } else {
                EndLogs();
            }
        }
    }


    public void PlayNextFrame() {
        if (animLoaded) {
            moshFrame++;
            if (!AnimDone) {
                Vector3 t = anim.GetTranslationAtFrame(moshFrame);
                anim.GetPose(pose, moshFrame);
                boneModifier.updateBoneAngles(pose, t);
                setPoseBlendValues();
            }
        }
    }


    public void JumpToFrame(int frame) {
        if (animLoaded && 0 <= frame && frame < anim.GetResampledTotalFrameCount) 
        {
            moshFrame = frame;
            if (!AnimDone) 
            {
                Vector3 t = anim.GetTranslationAtFrame(moshFrame);
                anim.GetPose(pose, moshFrame);
                boneModifier.updateBoneAngles(pose, t);
                setPoseBlendValues();
            }
        }
    }


    void setPoseBlendValues()
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
                meshRenderer.SetBlendShapeWeight(anim.ShapeBlendCount + (idx * 2) + elem + 0, pos * 100.0f);
                meshRenderer.SetBlendShapeWeight(anim.ShapeBlendCount + (idx * 2) + elem + 1, neg * 100.0f);
            }
        }
    }


    /// <summary>
    /// Gets the new joint positions from the animation.
    /// Passes them to the boneModifier. 
    /// </summary>
    public void calculateJoints()
    {
        Vector3[] joints = anim.GetJoints();
        boneModifier.updateBonePositions(joints, true);
    }

    /// <summary>
    /// Set the values of the first 20 blendshapes in the skinned 
    /// mesh renderer, defining body shape. 
    /// </summary>
    /// <param name="bweights">Values assigned to blendshapes.</param>
    public void setBetaBlshapeValues(float[] bweights) 
    {
        //!!!! float beta = betas[i] / SCALE; <- this was in original. It's important!!!
        Debug.Assert(bweights.Length == 20, "bweights array too small.");
        for (int i = 0; i < 20; i++) {
            meshRenderer.SetBlendShapeWeight(i, bweights[i]);
        }
    }


    private void resetBlendShapes() {
        for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++) {
            meshRenderer.SetBlendShapeWeight(i, 0f);
        }
    }


    /// <summary>
    /// Called by PlayAnim to reset the skeleton before playing another animation.
    /// </summary>
    public void reset()
    {
        if (LoggingVerts || LoggingJoints) {
            EndLogs();
        }

        boneModifier.ResetRotations();
        resetBlendShapes();
        Vector3[] joints = JointCalculator.GetDefaultJoints(anim.GetGender);
        boneModifier.updateBonePositions(joints, true);
        //boneModifier.reset();
        //resetJoints();
        animLoaded = false;
    }


    /// <summary>
    /// Set the models joints to the default values from the regressor. 
    /// </summary>
    private void resetJoints()
    {
        //boneModifier.ResetRotations();
        //boneModifier.reset_light();
        // or move this to a method in the model. 
        Vector3[] joints = JointCalculator.GetDefaultJoints(anim.GetGender);
        boneModifier.updateBonePositions(joints, true);
    }

    /// <summary>
    /// Activate the mesh for the specific gender and assigns it to the 
    /// skinned mesh renderer. 
    /// The mesh is first cloned if this has not previously been done.
    /// </summary>
    /// <param name="g">GetGender of mesh to swap in</param>
    void activateMesh(Gender g) 
    {
        if (g == Gender.FEMALE) {
            if (!meshCloned_f) {
                smpl_f = Instantiate<Mesh>(smpl_f);
                meshCloned_f = true;
            }
            // don't want to do this if using editor functionality, because the 
            // variable meshRenderer will not have been initialized. 
            if (Application.isPlaying) {
                //Debug.Log("is playing. not editor code.");
                meshRenderer.sharedMesh = smpl_f;
            }
        }

        else {
            if (!meshCloned_m) {
                smpl_m = Instantiate<Mesh>(smpl_m);
                meshCloned_m = true;
            }
            if (Application.isPlaying) {
                //Debug.Log("is playing. not editor code.");
                meshRenderer.sharedMesh = smpl_m;
            }
        }
    }


	private void OnApplicationQuit()
	{
        EndLogs();
	}

}