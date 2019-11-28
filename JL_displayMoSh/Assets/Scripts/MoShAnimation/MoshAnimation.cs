using UnityEngine;
using System;
using System.Text;


/// <summary>
/// This needs to return:
/// beta values
/// joint rotations
/// initial joint positions etc.
/// 
/// Using an abstract base class that can be extended with different backends JSON, a binary format etc. 
/// </summary>
public class MoshAnimation {

    readonly int           sourceTotalFrameCount;
    readonly Vector3[]     translations;
    readonly Quaternion[,] allPoses;
    readonly float[]       betas;

    int desiredFPS;

    int GetResampledTotalFrameCount { get; set; }

    readonly float  duration;
    public   Gender Gender { get; }

    readonly JointCalculator jointCalculator;
    bool                     resamplingRequired = false;
    readonly int             sourceFPS;
    BMLModifyBones           boneModifier;

    int                 currentFrame = 0;
    SkinnedMeshRenderer meshRenderer;

    /// <summary>
    /// Has the current animation finished playing, if one has been loaded.
    /// </summary>
    public bool Finished => currentFrame >= GetResampledTotalFrameCount;
    

    public MoshAnimation(Gender    gender, int sourceTotalFrameCount, int sourceFPS, float[] betas,
                         Vector3[] translations, Quaternion[,] allPoses, SMPLSettings settings) {
        Gender = gender;
        switch (gender) {
            case Gender.MALE:
                jointCalculator = settings.MaleJointCalculator;
                break;
            case Gender.Female:
                jointCalculator = settings.FemaleJointCalculator;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
        }

        this.sourceTotalFrameCount = sourceTotalFrameCount;
        this.sourceFPS = sourceFPS;
        desiredFPS = sourceFPS;
        GetResampledTotalFrameCount = sourceTotalFrameCount;
        duration = this.sourceTotalFrameCount / (float) this.sourceFPS;
        this.betas = betas;
        this.translations = translations;
        this.allPoses = allPoses;
        currentFrame = 0;

    }

    public void AttachAnimationToMoshCharacter(SkinnedMeshRenderer meshRendererToAttach) {
        meshRenderer = meshRendererToAttach;
        boneModifier = new BMLModifyBones(meshRenderer);
        
        //TODO make it reset AFTERWARDS, not before.
        Reset();
        
        //Set Betas of avg FBX model in the scene to shapeBetas from Mosh file
        UpdateBlendShapes();
        //Calculate INITIAL joint-locations from shapeBetas & update joints of the FBX model
        CalculateJoints();
        

    }


    public void AdjustFrameRate(int desiredFrameRate) {
        if (desiredFrameRate != 0) {
            SetDesiredFPS(desiredFrameRate);
        }
    }


    /// <summary>
    /// Gets or sets the fps, upsampling or downsampling if the fps is 
    /// is different from the source fps. 
    /// </summary>
    /// <value>The new setDesiredFPS to sample to.</value>
    public void SetDesiredFPS(int value) {
        
        desiredFPS = value;
        // duration stays constant, but upsampling/downsampling will happen.
        // Time of start and end keys remains constant, but keys in between are shifted
        // and more may be added or removed.
        
        resamplingRequired = desiredFPS != sourceFPS;
        
        // have to update length here. 
        // I think this is the right way to get length.
        // actually, since the time of the last thisFrameAsDecimal should remain static, 
        // if the time between frames is a constant, then the time of the last thisFrameAsDecimal cannot
        // be completely static. 
        // I think I should still floor the value. 
        GetResampledTotalFrameCount = Mathf.FloorToInt(desiredFPS * duration);
        
    }


    Vector3 GetTranslationAtFrame(int thisFrame) {
        // so the original code flips the translations, if the up axis is equal to z. 
        // I guess I should check on that. 
        
        if (!resamplingRequired) return translations[thisFrame];
        
        float percentageElapsedSinceLastFrame = PercentageElapsedBetweenFrames(thisFrame, out int frameBeforeThis, out int frameAfterThis);
        
        bool lastFrameInAnimation = frameAfterThis >= sourceTotalFrameCount;
        if (lastFrameInAnimation) { 
            return translations[frameBeforeThis];
        }

        Vector3 resampledTranslation = Vector3.Lerp(translations[frameBeforeThis], translations[frameAfterThis], percentageElapsedSinceLastFrame);
        return resampledTranslation;
    }

    float PercentageElapsedBetweenFrames(int thisFrame, out int frameBeforeThis, out int frameAfterThis) {
        float timeFrameOccurs = GetTimeAtFrame(thisFrame);
        
        float decimalFrameIndex = sourceFPS * timeFrameOccurs;
        frameBeforeThis = Mathf.FloorToInt(decimalFrameIndex);
        frameAfterThis = Mathf.CeilToInt(decimalFrameIndex);
        float percentageElapsedSinceLastFrame = decimalFrameIndex - frameBeforeThis;
        return percentageElapsedSinceLastFrame;
    }

    /// <summary>
    /// Get the time, in seconds since start of animation, at a specified thisFrameAsDecimal.
    /// </summary>
    float GetTimeAtFrame(int frame) {
        float percentComplete = frame / (float)GetResampledTotalFrameCount;
        float timeAtFrame = percentComplete * duration;
        return timeAtFrame;
    }

    /// <summary>
    /// Populate an array with rotations of each joint at thisFrameAsDecimal. 
    /// </summary>
    /// <param name="rotations">Array to fill with joint rotations.</param>
    /// <param name="thisFrameAsDecimal">Frame at which to get rotations</param>
    Quaternion[] GetPoseAtFrame(int thisFrameAsDecimal) 
    {
        Quaternion[] thisFramePoses = new Quaternion[SMPL.JointCount];
        
        // ok. Need to spherically interpolate all these quaternions. 
        for (int jointIndex = 0; jointIndex < SMPL.JointCount; jointIndex++) {
            
            if (!resamplingRequired) {
                // these local rotations are in the right coordinate system for unity.
                thisFramePoses[jointIndex] = allPoses[thisFrameAsDecimal, jointIndex];
            }
            else {
                float percentageElapsedBetweenFrames =
                    PercentageElapsedBetweenFrames(thisFrameAsDecimal, out int frameBeforeThis,
                                                   out int frameAfterThis);

                // detect last thisFrameAsDecimal. This might be a slight discontinuity. 
                if (frameAfterThis >= sourceTotalFrameCount) {
                    thisFramePoses[jointIndex] = allPoses[frameBeforeThis, jointIndex];
                }
                else {
                    Quaternion rotationAtFrameBeforeThis = allPoses[frameBeforeThis, jointIndex];
                    Quaternion rotationAtFrameAfterThis = allPoses[frameAfterThis, jointIndex];
                    thisFramePoses[jointIndex] = Quaternion.Slerp(rotationAtFrameBeforeThis, rotationAtFrameAfterThis,
                                                             percentageElapsedBetweenFrames);
                }
            }
        }

        return thisFramePoses;
    }

    /// <summary>
    /// Get the values for shape parameters in Unity, that define the 
    /// shape of the subject. 
    /// </summary>
    [Obsolete] float[] GetBetas () {
        float[] values = new float[SMPL.ShapeBetaCount];

        for (int i = 0; i < SMPL.ShapeBetaCount; i++) {
            float scaledBeta = ScaleBetaFromBlenderToUnity(betas[i]);
            values[i] = scaledBeta;
        }
        return values;
    }

    static float ScaleBetaFromBlenderToUnity(float beta) {
        float scaledBeta = beta * 100f / SMPL.BetaScalingFactor;
        return scaledBeta;
    }

    
    void SetPoseDependentBlendShapesForCurrentFrame(Quaternion[] poses) {
        // start at 1 to skip pelvis. 
        // pelvis has a rotation, but doesn't seem to have associated blend shapes.
        const int FirstJointAfterPelvis = 1;
        for (int jointPoseIndexAfterPelvis = FirstJointAfterPelvis; jointPoseIndexAfterPelvis < SMPL.JointCount; jointPoseIndexAfterPelvis++) {
            // i is equivalent to index for the other version. 
            Quaternion currentPose = poses[jointPoseIndexAfterPelvis];
            float[] rot3x3 = MoShUtilities.QuaternionTo3X3Matrix(currentPose);
            
            int jointPoseIndexAfterPelvisExcluded = jointPoseIndexAfterPelvis - 1;
            for (int rotationMatrixElementIndex = 0; rotationMatrixElementIndex < SMPL.RotationMatrixElementCount; rotationMatrixElementIndex++) {
                float pos, neg;
                float theta = rot3x3[rotationMatrixElementIndex];
                float scaledTheta = ScaleBetaFromBlenderToUnity(theta); 
                int index = SMPL.ShapeBetaCount + jointPoseIndexAfterPelvisExcluded * SMPL.RotationMatrixElementCount + rotationMatrixElementIndex;
                meshRenderer.SetBlendShapeWeight(index, scaledTheta);
            }
        }
    }
    
    
    public void PlayCurrentFrame() {
        Vector3 translationAtFrame = GetTranslationAtFrame(currentFrame);
        Quaternion[] poses = GetPoseAtFrame(currentFrame);
        boneModifier.UpdateBoneAngles(poses, translationAtFrame);
        SetPoseDependentBlendShapesForCurrentFrame(poses);
        currentFrame++;

        if (Finished) {
            Reset();
        }
    }
    
    /// <summary>
    /// Called by PlayAnim to reset the skeleton before playing another animation.
    /// </summary>
    void Reset() {
        if (!Finished) Debug.Log("Resetting but not finished");
        boneModifier.ResetRotations();
        ResetBlendShapes();
        Vector3[] joints = jointCalculator.CalculateJointsAtZeroedBetas();
        boneModifier.UpdateBonePositions(joints);
    }
    
    
    void UpdateBlendShapes() {
        float[] shapeBetas = GetBetas();
        for (int betaIndex = 0; betaIndex < SMPL.ShapeBetaCount; betaIndex++) {
            meshRenderer.SetBlendShapeWeight(betaIndex, shapeBetas[betaIndex]);
        }
    }
        
    void ResetBlendShapes() {
        for (int blendShapeIndex = 0; blendShapeIndex < meshRenderer.sharedMesh.blendShapeCount; blendShapeIndex++) {
            meshRenderer.SetBlendShapeWeight(blendShapeIndex, 0f);
        }
    }
    
    /// <summary>
    /// Gets the new joint positions from the animation.
    /// Passes them to the boneModifier. 
    /// </summary>
    void CalculateJoints()
    {
        Vector3[] joints = jointCalculator.CalculateJointPositions(betas);
        boneModifier.UpdateBonePositions(joints);
    }
    
    
    
}
