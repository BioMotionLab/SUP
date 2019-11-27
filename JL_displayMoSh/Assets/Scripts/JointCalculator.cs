using System;
using UnityEngine;
using LightweightMatrixCSharp;


public class JointCalculator {
    
    readonly Matrix[] template;
    readonly Matrix[] jointsRegressor; // doesn't need reset. 
    readonly Vector3[] joints;
    readonly SMPLSettings settings;


    // private constructor. 
    public JointCalculator(Matrix[] template, Matrix[] jointsRegressor, Vector3[] joints,  SMPLSettings settings) {
        this.template = template;
        this.jointsRegressor = jointsRegressor;
        this.settings = settings;
        this.joints = joints;

    }


    /// <summary>
    /// Calculate initial joint positions from betas. 
    /// </summary>
    /// <returns>The joints.</returns>
    public Vector3[] CalculateJoints(float[] betas) {
        // Create beta value matrix
        Matrix betaMatrix = new Matrix(SMPL.ShapeBetaCount, 1); // column vector.
        
        for (int row = 0; row < SMPL.ShapeBetaCount; row++) {
            betaMatrix[row, 0] = betas[row];
        }

        Matrix newJointsX = jointsRegressor[0] * betaMatrix + template[0];
        Matrix newJointsY = jointsRegressor[1] * betaMatrix + template[1];
        Matrix newJointsZ = jointsRegressor[2] * betaMatrix + template[2];

        // Update joints vector
        for (int jointIndex = 0; jointIndex < SMPL.JointCount; jointIndex++) {
            
            Vector3 convertFromMaya = new Vector3(
                                                  (float)newJointsX[jointIndex, 0], 
                                                  (float)newJointsY[jointIndex, 0], 
                                                  (float)newJointsZ[jointIndex, 0]);
            
            joints[jointIndex] = ConvertFromMayaCoordinateSystem(convertFromMaya);
        }
        return joints;
    }

    static Vector3 ConvertFromMayaCoordinateSystem(Vector3 inMayaCoordinates) {
        // Convert Maya regressor to Unity coordinate system by negating X value
        Vector3 inUnityCoordinates = new Vector3(
                                          -inMayaCoordinates.x, 
                                          inMayaCoordinates.y,
                                          inMayaCoordinates.z);
        return inUnityCoordinates;
    }
    


    public Vector3[] GetDefaultJoints(Gender g) {
        JointCalculator jc;
        switch (g) {
            case Gender.MALE:
                jc = settings.MaleJointCalculator;
                break;
            case Gender.Female:
                jc = settings.FemaleJointCalculator;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(g), g, null);
        }
        float[] b0 = new float[10];
        return jc.CalculateJoints(b0);
    }


}