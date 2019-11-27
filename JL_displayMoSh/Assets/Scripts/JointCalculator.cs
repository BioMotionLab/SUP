using System;
using UnityEngine;
using LightweightMatrixCSharp;
using SimpleJSON;


public class JointCalculatorFromJSON {
    Matrix[] jointsRegressor;
    Vector3[] joints;
    Matrix[] template;


    public JointCalculatorFromJSON(TextAsset jsonText) {
        joints = new Vector3[SMPL.JointCount];
        template = new Matrix [SMPL.JointDimensions];
        jointsRegressor = new Matrix [SMPL.JointDimensions];

        JSONNode mainNode = JSON.Parse(jsonText.text);
        
        ParseTemplatesFromJSON(mainNode);
        ParseJointRegressorsFromJSON(mainNode);
    }
    
    void ParseJointRegressorsFromJSON(JSONNode node) {
        JSONNode betasJointRegressorNode = node[SMPL.JSONKeys.BetaJointRegressors];
        for (int dimensionIndex = 0; dimensionIndex < SMPL.JointDimensions; dimensionIndex++) {
            jointsRegressor[dimensionIndex] = new Matrix(SMPL.JointCount, SMPL.ShapeBetaCount);
        }

        for (int jointIndex = 0; jointIndex < SMPL.JointCount; jointIndex++) {
            // Init joint template matrix
            // Init beta regressor matrix    
            for (int shapeBetaIndex = 0; shapeBetaIndex < SMPL.ShapeBetaCount; shapeBetaIndex++) {
                for (int dimensionIndex = 0; dimensionIndex < SMPL.JointDimensions; dimensionIndex++) {
                    Matrix jointRegressorMatrix = jointsRegressor[dimensionIndex];
                    jointRegressorMatrix[jointIndex, shapeBetaIndex] =
                        betasJointRegressorNode[jointIndex][dimensionIndex][shapeBetaIndex].AsDouble;
                }
            }
        }
    }

    void ParseTemplatesFromJSON(JSONNode node) {
        JSONNode templateNode = node[SMPL.JSONKeys.Templates];
        for (int dimensionIndex = 0; dimensionIndex < SMPL.JointDimensions; dimensionIndex++) {
            template[dimensionIndex] = new Matrix(SMPL.JointCount, 1);
        }

        for (int jointIndex = 0; jointIndex < SMPL.JointCount; jointIndex++) {
            // Init joint template matrix

            for (int dimensionIndex = 0; dimensionIndex < SMPL.JointDimensions; dimensionIndex++) {
                double x = templateNode[jointIndex][dimensionIndex].AsDouble;
                Matrix templateMatrix = template[dimensionIndex];
                templateMatrix[jointIndex, 0] = x;
            }
        }
    }

    public JointCalculator BuildWithSettings(SMPLSettings settings) {
        return new JointCalculator(template, jointsRegressor, joints, settings);
    }
}


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
            
            joints[jointIndex] = ConvertFromMayaCoordinateSystem(jointIndex, newJointsX, newJointsY, newJointsZ);
        }
        return joints;
    }

    static Vector3 ConvertFromMayaCoordinateSystem(int rowIndex, Matrix newJointsX, Matrix newJointsY, Matrix newJointsZ) {
        // Convert Maya regressor to Unity coordinate system by negating X value
        Vector3 convertFromMaya = new Vector3(
                                          -(float)newJointsX[rowIndex, 0], 
                                          (float)newJointsY[rowIndex, 0], 
                                          (float)newJointsZ[rowIndex, 0]);
        return convertFromMaya;
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