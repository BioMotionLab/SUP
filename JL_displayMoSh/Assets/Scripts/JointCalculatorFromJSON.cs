using LightweightMatrixCSharp;
using SimpleJSON;
using UnityEngine;

public class JointCalculatorFromJSON {
    
    readonly Matrix[]  jointsRegressor;
    readonly Vector3[] joints;
    readonly Matrix[]  template;
    
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