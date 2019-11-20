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
public abstract class MoShAnimation {

    // a scale variable is needed in order to calculate the beta values.
    const float BetaScalingFactor = 5.0f;

    protected int SourceFPS;
    protected int SourceLength;

    protected Vector3[] Translation;
    protected Quaternion[,] Poses;
    protected float[] betas;

    bool ResamplingRequired = false;

    protected const bool ZAxisUp = true;

    protected JointCalculator Jc;

    protected int length;
    
    /// <summary>
    /// Read only. Get the number of frames in the animation. 
    /// </summary>
    /// <value>length of the animation</value>
    public int Length => length;

    protected float duration;

    protected Genders gender;
    public Genders Gender => gender;


    // these should be fixed to be more consistent. 
    public const int BetaCount = 10;
    const int blendCount = shapeBlendCount + poseBlendCount;
    const int shapeBlendCount = 2 * BetaCount;
    public int ShapeBlendCount => shapeBlendCount;
    const int poseBlendCount = 207 * 2;
    public const int JointCount = 24;



    protected int fps;

    /// <summary>
    /// Gets or sets the fps, upsampling or downsampling if the fps is 
    /// is different from the source fps. 
    /// </summary>
    /// <value>The new FPS to sample to.</value>
    public int FPS {
        set {
            fps = value;
            // duration stays constant, but upsampling/downsampling will happen.
            // Time of start and end keys remains constant, but keys in between are shifted
            // and more may be added or removed.
            
            if (fps != SourceFPS) {
                ResamplingRequired = true;
            } else {
                ResamplingRequired = false;
            }
            
            // have to update length here. 
            // I think this is the right way to get length.
            // actually, since the time of the last thisFrame should remain static, 
            // if the time between frames is a constant, then the time of the last thisFrame cannot
            // be completely static. 
            // I think I should still floor the value. 
            length = Mathf.FloorToInt(fps * duration);
        }
    }


    public Vector3 GetTranslationAtFrame(int thisFrame) 
    {
        // so the original code flips the translation, if the up axis is equal to z. 
        // I guess I should check on that. 
        
        if (!ResamplingRequired) return Translation[thisFrame];
        
        float percentageElapsedSinceLastFrame = PercentageElapsedSinceLastFrame(thisFrame, out int frameBeforeThis, out int frameAfterThis);
        
        bool lastFrameInAnimation = frameAfterThis >= SourceLength;
        if (lastFrameInAnimation) { 
            return Translation[frameBeforeThis];
        }

        Vector3 resampledTranslation = Vector3.Lerp(Translation[frameBeforeThis], Translation[frameAfterThis], percentageElapsedSinceLastFrame);
        return resampledTranslation;
    }

    float PercentageElapsedSinceLastFrame(int thisFrame, out int frameBeforeThis, out int frameAfterThis)
    {
        float timeFrameOccurs = GetTimeAtFrame(thisFrame);
        
        float decimalFrameIndex = SourceFPS * timeFrameOccurs;
        frameBeforeThis = Mathf.FloorToInt(decimalFrameIndex);
        frameAfterThis = Mathf.CeilToInt(decimalFrameIndex);
        float percentageElapsedSinceLastFrame = decimalFrameIndex - frameBeforeThis;
        return percentageElapsedSinceLastFrame;
    }

    /// <summary>
    /// Get the time, in seconds since start of animation, at a specified thisFrame.
    /// </summary>
    float GetTimeAtFrame(int frame) 
    {
        float percentComplete = frame / (float)length;
        float timeAtFrame = percentComplete * duration;
        return timeAtFrame;
    }

    /// <summary>
    /// Populate an array with rotations of each joint at thisFrame. 
    /// </summary>
    /// <param name="rotations">Array to fill with joint rotations.</param>
    /// <param name="frame">Frame at which to get rotations</param>
    public void GetPose(Quaternion[] rotations, int frame) 
    {
        if (rotations == null) {
            throw new Exception("null array passed to GetPose");
        } else if (rotations.Length != JointCount) {
            throw new Exception("array with wrong length passed to get pose");
        }
        // ok. Need to spherically interpolate all these quaternions. 

        if (ResamplingRequired) {
            for (int i = 0; i < JointCount; i++) {
                float p = PercentageElapsedSinceLastFrame(frame, out int f1, out int f2);

                // detect last thisFrame. This might be a slight discontinuity. 
                if (f2 >= SourceLength) {
                    rotations[i] = Poses[f1, i];
                }
                else {
                    Quaternion q1 = Poses[f1, i];
                    Quaternion q2 = Poses[f2, i];
                    rotations[i] = Quaternion.Slerp(q1, q2, p);
                }
            }
        }
        else {
            for (int i = 0; i < JointCount; i++) {
                // these local rotations are in the right coordinate system for unity.
                rotations[i] = Poses[frame, i];
            }
        }
    }

    /// <summary>
    /// Get the values for the first 20 blend shapes in Unity, that define the 
    /// shape of the subject. 
    /// Shape: first 20 blend shapes. Doesn't change over time. 
    /// </summary>
    /// <param name="values">Values.</param>
    public virtual void GetShapeBlendValues (float[] values) 
    {
        if (values.Length != shapeBlendCount) throw new Exception("Array values too small. Must have length 20.");
        
        for (int i = 0; i < BetaCount; i++) {
            float scaledBeta = ScaleBeta(betas[i]);
            
            //Because of pos and neg
            int doubledIndex = 2 * i;
            if (betas[i] >= 0) {
                values[doubledIndex] = scaledBeta;
                values[doubledIndex + 1] = 0f;
            } else {
                values[doubledIndex] = 0f;
                values[doubledIndex + 1] = -scaledBeta;
            }
        }
    }

    float ScaleBeta(float beta) {
        float scaledBeta = beta * 100f / BetaScalingFactor;
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
        return Jc.calculateJoints(betas);
    }
    
}
