using UnityEngine;
using System;


/// <summary>
/// This needs to return:
/// beta values
/// joint rotations
/// initial joint positions etc.
/// 
/// Using an abstract base class that can be extended with different backends JSON, a binary format etc. 
/// </summary>
public class MoshAnimation {

    readonly int sourceTotalFrameCount;
    readonly Vector3[] translations;
    readonly Quaternion[,] poses;
    readonly float[] betas;
    
    int desiredFPS;
    
    /// <summary>
    /// Read only. Get the number of frames in the animation. 
    /// </summary>
    /// <value>length of the animation</value>
    public int GetResampledTotalFrameCount => resampledTotalFrameCount;

    readonly float duration;
    public Gender Gender { get; }
    
    readonly JointCalculator jointCalculator;
    bool            resamplingRequired = false;
    int             resampledTotalFrameCount;
    readonly int             sourceFPS;
    MoshCharacter moshCharacter;


    public MoshAnimation(Gender gender,  int sourceTotalFrameCount, int sourceFPS, float[] betas,
                Vector3[] translations, Quaternion[,] poses) {
                this.Gender = gender;
                switch (gender) {
                    case Gender.MALE:
                    jointCalculator = JointCalculator.Male;
                    break;
                    case Gender.Female:
                    jointCalculator = JointCalculator.Female;
                    break;
                    default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
                }
                this.sourceTotalFrameCount = sourceTotalFrameCount;
                this.sourceFPS = sourceFPS;
                desiredFPS = sourceFPS;
                resampledTotalFrameCount = sourceTotalFrameCount;
                duration = this.sourceTotalFrameCount / (float)this.sourceFPS;
                this.betas = betas;
                this.translations = translations;
                this.translations = translations;
                this.poses = poses;
                
    }

    public void AttachAnimationToMoshCharacter(MoshCharacter moshCharacter) {
        this.moshCharacter = moshCharacter;
        //Set Betas of avg FBX model in the scene to shapeBetas from Mosh file
        SetMeshShapeBetas();

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
        
        if (desiredFPS != sourceFPS) {
            resamplingRequired = true;
        } else {
            resamplingRequired = false;
        }
        
        // have to update length here. 
        // I think this is the right way to get length.
        // actually, since the time of the last thisFrameAsDecimal should remain static, 
        // if the time between frames is a constant, then the time of the last thisFrameAsDecimal cannot
        // be completely static. 
        // I think I should still floor the value. 
        resampledTotalFrameCount = Mathf.FloorToInt(desiredFPS * duration);
        
    }


    public Vector3 GetTranslationAtFrame(int thisFrame) 
    {
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

    float PercentageElapsedBetweenFrames(int thisFrame, out int frameBeforeThis, out int frameAfterThis)
    {
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
    float GetTimeAtFrame(int frame) 
    {
        float percentComplete = frame / (float)resampledTotalFrameCount;
        float timeAtFrame = percentComplete * duration;
        return timeAtFrame;
    }

    /// <summary>
    /// Populate an array with rotations of each joint at thisFrameAsDecimal. 
    /// </summary>
    /// <param name="rotations">Array to fill with joint rotations.</param>
    /// <param name="thisFrameAsDecimal">Frame at which to get rotations</param>
    public Quaternion[] GetPoseAtFrame(int thisFrameAsDecimal) 
    {
        Quaternion[] posesThisFrame = new Quaternion[SMPL.JointCount];
        
        if (posesThisFrame == null) throw new NullReferenceException("null array passed to GetPoseAtFrame");
        if (posesThisFrame.Length != SMPL.JointCount) throw new IndexOutOfRangeException("array with wrong length passed to get pose");
        
        // ok. Need to spherically interpolate all these quaternions. 
        for (int jointIndex = 0; jointIndex < SMPL.JointCount; jointIndex++) {
            
            if (!resamplingRequired) {
                // these local rotations are in the right coordinate system for unity.
                posesThisFrame[jointIndex] = poses[thisFrameAsDecimal, jointIndex];
            }
            else {
                float percentageElapsedBetweenFrames =
                    PercentageElapsedBetweenFrames(thisFrameAsDecimal, out int frameBeforeThis,
                                                   out int frameAfterThis);

                // detect last thisFrameAsDecimal. This might be a slight discontinuity. 
                if (frameAfterThis >= sourceTotalFrameCount) {
                    posesThisFrame[jointIndex] = poses[frameBeforeThis, jointIndex];
                }
                else {
                    Quaternion rotationAtFrameBeforeThis = poses[frameBeforeThis, jointIndex];
                    Quaternion rotationAtFrameAfterThis = poses[frameAfterThis, jointIndex];
                    posesThisFrame[jointIndex] = Quaternion.Slerp(rotationAtFrameBeforeThis, rotationAtFrameAfterThis,
                                                             percentageElapsedBetweenFrames);
                }
            }
        }

        return posesThisFrame;
    }

    /// <summary>
    /// Get the values for shape parameters in Unity, that define the 
    /// shape of the subject. 
    /// Shape: first several blend shapes. Doesn't change over time. 
    /// </summary>
    [Obsolete] public float[] GetDoubledBetas () {
        float[] values = new float[SMPL.DoubledShapeBetaCount];
        
        for (int i = 0; i < SMPL.ShapeBetaCount; i++) {
            float scaledBeta = ScaleBeta(betas[i]);
            //Because of pos and neg
            int doubledIndex = 2 * i;
            if (betas[i] >= 0) {
                values[doubledIndex] = scaledBeta;
                values[doubledIndex + 1] = 0f;
            } 
            else {
                values[doubledIndex] = 0f;
                values[doubledIndex + 1] = -scaledBeta;
            }
        }
        return values;
    }

    float ScaleBeta(float beta) {
        float scaledBeta = beta * 100f / SMPL.BetaScalingFactor;
        return scaledBeta;
    }

    /// <summary>
    /// Get the initial joint positions computed using the beta values read from the
    /// animation file.
    /// </summary>
    /// <returns>The joints.</returns>
    public Vector3[] GetJoints() {
        // It may be a good idea to clone this array or something. This will be 
        // a reference to the array stored in JointCalculator. It's probably
        // not good for other things to be referencing the array in JointCalculator,
        // but if they are, this function might override values that are depended on. 
        return jointCalculator.calculateJoints(betas);
    }
    
    /// <summary>
    /// Set the values of the first 20 blendshapes in the skinned 
    /// mesh renderer, defining body shape. 
    /// </summary>
    /// <param name="shapeBetas">Values assigned to blendshapes.</param>
    void SetMeshShapeBetas() {
        float[] shapeBetas = GetDoubledBetas();
        //!!!! float beta = shapeBetas[i] / SCALE; <- this was in original. It's important!!!
        for (int betaIndex = 0; betaIndex < SMPL.DoubledShapeBetaCount; betaIndex++) {
            moshCharacter.MeshRenderer.SetBlendShapeWeight(betaIndex, shapeBetas[betaIndex]);
        }
    }
    
}
