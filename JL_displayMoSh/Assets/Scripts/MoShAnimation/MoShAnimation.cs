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
    public float SCALE = 5.0f;

    protected int sourceFPS;
    protected int sourceLength;

    protected Vector3[] translation;
    protected Quaternion[,] poses;
    protected float[] betas;

    //private Vector3[] joints = new Vector3[JointCount];

    protected bool sampling = false;

    protected int fps;

    protected bool zAxisUp = true;

    protected JointCalculator jc;

    protected int length;
    /// <summary>
    /// Read only. Get the number of frames in the animation. 
    /// </summary>
    /// <value>length of the animation</value>
    public virtual int Length {
        get {
            return length;
        }
    }

    protected float duration;
    /// <summary>
    /// Duration of the animation in seconds. 
    /// </summary>
    public virtual float Duration {
        get {
            return duration;
        }
    }

    protected Genders gender;
    public virtual Genders Gender {
        get {
            return gender;
        }
    }

    /// <summary>
    /// Read only. The raw beta values from the animation file. (See SMPL paper)
    /// </summary>
    /// <value>The betas.</value>
    public float[] Betas {
        get {
            //float[] b = new float[10];
            //for (int i = 0; i < 10; i++) {
            //    b[i] = betas[i] / SCALE;
            //}
            //return b;
            return betas;
        }
    }


    // these should be fixed to be more consistent. 
    public const int BetaCount = 10;
    public const int blendCount = shapeBlendCount + poseBlendCount;
    public int BlendCount {
        get {
            return blendCount;
        }
    }
    public const int shapeBlendCount = 2 * BetaCount;
    public int ShapeBlendCount {
        get {
            return shapeBlendCount;
        }
    }


    public const int poseBlendCount = 207 * 2;
    public int PoseBlendCount {
        get {
            return poseBlendCount;
        }
    }

    //public const int jointCount = 24;
    public const int JointCount = 24;
    //public int JointCount {
    //    get {
    //        return nJoints;
    //    }
    //}


    /// <summary>
    /// Gets or sets the fps, upsampling or downsampling if the fps is 
    /// is different from the source fps. 
    /// </summary>
    /// <value>The new FPS to sample to.</value>
    public virtual int FPS {
        get {
            return fps;
        }
        set {
            fps = value;
            // duration stays constant, but upsampling/downsampling will happen.
            // Time of start and end keys remains constant, but keys in between are shifted
            // and more may be added or removed.
            if (fps != sourceFPS) {
                sampling = true;
            } else {
                sampling = false;
            }
            //Debug.Log("changed fps");
            // have to update length here. 
            // I think this is the right way to get length.
            // actually, since the time of the last frame should remain static, 
            // if the time between frames is a constant, then the time of the last frame cannot
            // be completely static. 
            // I think I should still floor the value. 
            length = Mathf.FloorToInt(fps * duration);
        }
    }


    public virtual Vector3 GetTranslation (int frame) 
    {
        // so the original code flips the translation, if the up axis is equal to z. 
        // I guess I should check on that. 
        if (sampling) {
            int f1, f2;
            float p = InterpParameter(frame, out f1, out f2);
            // last frame in animation.
            if (f2 >= sourceLength) { 
                return translation[f1];
            }
            return Vector3.Lerp(translation[f1], translation[f2], p);
        }
        return translation[frame];
    }

    /// <summary>
    /// Populate an array with rotations of each joint at frame. 
    /// </summary>
    /// <param name="rotations">Array to fill with joint rotations.</param>
    /// <param name="frame">Frame at which to get rotations</param>
    public virtual void GetPose(Quaternion[] rotations, int frame) 
    {
        if (rotations == null) {
            throw new Exception("null array passed to GetPose");
        } else if (rotations.Length != JointCount) {
            throw new Exception("array with wrong length passed to get pose");
        }
        // ok. Need to spherically interpolate all these quaternions. 

        if (sampling) {
            Quaternion q1, q2;
            for (int i = 0; i < JointCount; i++) {
                int f1, f2;
                float p = InterpParameter(frame, out f1, out f2);

                // detect last frame. This might be a slight discontinuity. 
                if (f2 >= sourceLength) {
                    rotations[i] = poses[f1, i];
                }
                else {
                    q1 = poses[f1, i];
                    q2 = poses[f2, i];
                    rotations[i] = Quaternion.Slerp(q1, q2, p);
                }
            }
        }
        else {
            for (int i = 0; i < JointCount; i++) {
                // these local rotations are in the right coordinate system for unity.
                rotations[i] = poses[frame, i];
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
        if (values.Length != 20) {
            throw new Exception("Array values too small. Must have length 20.");
        }
        for (int i = 0; i < 10; i++) 
        {
            if (betas[i] >= 0) {
                values[2 * i] = betas[i] * 100f / SCALE;
                values[(2 * i) + 1] = 0f;
            } else {
                values[2 * i] = 0f;
                values[(2 * i) + 1] = -1 * betas[i] * 100f / SCALE;
            }
        }
    }

    //public virtual void GetPoseBlendValues(float[] values, int frame) 
    //{
    //    // first bl shape: 0. 
    //    // last bl shape: 206.
    //    // Debug.Assert(values.Length > )
    //    // I need to interpolate for this as well. 
    //    // actually, I should do this in the MoShAnimPlayer class. 
    //    float[] rmat;
    //    for (int i = 0; i < JointCount; i++) {
    //        rmat = MoShUtilities.Quat_to_3x3Mat(poses[frame, i]);
    //        // now I have to extract the elements and return them. 
    //        // won't be very difficult. 
    //    }
    //}

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
        return jc.calculateJoints(betas);
    }

    // frame - a frame index in the sampled new framerate. 
    // ia, ib - closest frames at original frame rate. 
    protected float InterpParameter(int frame, out int ia, out int ib)
    {
        // find the time at which frame occurs.
        float time = getTimeAtFrame(frame);
        // now use source length to find the nearest original frames. 
        float f = sourceFPS * time;
        ia = Mathf.FloorToInt(f);
        ib = Mathf.CeilToInt(f);
        //Debug.Log("time " + time + " f " + f + " ia " + ia + " ib " + ib);
        return f - ia; // return fractional component of a. 
    }


    protected static float ComputeDuration(int len, int cfps) 
    {
        float deltaT = 1f / cfps;
        return len * deltaT;
    }

    /// <summary>
    /// Get the time, in seconds since start of animation, at a specified frame.
    /// </summary>
    protected float getTimeAtFrame(int key) 
    {
        // key -> [0, 1] -> [0, duration]
        float p = key / (float)length;
        //string pstr = string.Format("P = {0}")
        //Debug.Log("P " + p);
        return p * duration;
    }

}
