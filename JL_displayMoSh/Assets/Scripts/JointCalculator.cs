using System;
using UnityEngine;
using LightweightMatrixCSharp;


public class JointCalculatorFromJSON {
    
    
    
    
    
}


/// <summary>
/// Double singleton implementation of joint calculator.
/// </summary>
public class JointCalculator {
    
    readonly Matrix[] template;
    readonly Matrix[] jointsRegressor; // doesn't need reset. 
    readonly Vector3[] joints;
    SMPLSettings settings;


    // private constructor. 
    public JointCalculator(TextAsset jsonText, SMPLSettings settings) {
        this.settings = settings;
        joints = new Vector3[SMPL.JointCount];
        template = new Matrix [3];
        jointsRegressor = new Matrix [3];

        for (int i = 0; i <= 2; i++) {
            template[i] = new Matrix(SMPL.JointCount, 1);
            jointsRegressor[i] = new Matrix(SMPL.JointCount, SMPL.ShapeBetaCount);
        }

        SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(jsonText.text);

        for (int i = 0; i < SMPL.JointCount; i++) {
            // Init joint template matrix
            double x = node["template_J"][i][0].AsDouble;
            double y = node["template_J"][i][1].AsDouble;
            double z = node["template_J"][i][2].AsDouble;

            (template[0])[i, 0] = x;
            (template[1])[i, 0] = y;
            (template[2])[i, 0] = z;

            // Init beta regressor matrix    
            for (int j = 0; j < SMPL.ShapeBetaCount; j++) {
                (jointsRegressor[0])[i, j] = node["betasJ_regr"][i][0][j].AsDouble;
                (jointsRegressor[1])[i, j] = node["betasJ_regr"][i][1][j].AsDouble;
                (jointsRegressor[2])[i, j] = node["betasJ_regr"][i][2][j].AsDouble;
            }
        }
    }

    /// <summary>
    /// Calculate initial joint positions from betas. 
    /// </summary>
    /// <returns>The joints.</returns>
    public Vector3[] calculateJoints(float[] betas) {
        // Create beta value matrix
        Matrix betaMatrix = new Matrix(SMPL.ShapeBetaCount, 1); // column vector.
        for (int row = 0; row < SMPL.ShapeBetaCount; row++) {
            betaMatrix[row, 0] = betas[row];
        }

        Matrix newJointsX = jointsRegressor[0] * betaMatrix + template[0];
        Matrix newJointsY = jointsRegressor[1] * betaMatrix + template[1];
        Matrix newJointsZ = jointsRegressor[2] * betaMatrix + template[2];

        // Update joints vector
        for (int row = 0; row < SMPL.JointCount; row++) {
            // Convert Maya regressor to Unity coordinate system by negating X value
            joints[row] = new Vector3(-(float)newJointsX[row, 0], (float)newJointsY[row, 0], (float)newJointsZ[row, 0]);
        }
        return joints;
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
        return jc.calculateJoints(b0);
    }


}