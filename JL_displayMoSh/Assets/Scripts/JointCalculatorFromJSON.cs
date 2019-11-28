using LightweightMatrixCSharp;
using SimpleJSON;
using UnityEngine;

public class JointCalculatorFromJSON {
    
    readonly Matrix[]  jointsRegressor;
    readonly Matrix[]  template;

    public JointCalculatorFromJSON(TextAsset jsonText) {
        template = new Matrix [SMPL.DimensionsOfAVector3];
        jointsRegressor = new Matrix [SMPL.DimensionsOfAVector3];

        JSONNode mainNode = JSON.Parse(jsonText.text);
        
        ParseTemplatesFromJSON(mainNode);
        ParseJointRegressorsFromJSON(mainNode);
    }
    
    void ParseJointRegressorsFromJSON(JSONNode node) {
        JSONNode betasJointRegressorNode = node[SMPL.JSONKeys.BetaJointRegressors];
        
        for (int vector3Dimension = 0; vector3Dimension < SMPL.DimensionsOfAVector3; vector3Dimension++) {
            jointsRegressor[vector3Dimension] = new Matrix(SMPL.JointCount, SMPL.ShapeBetaCount);
        }

        for (int jointIndex = 0; jointIndex < SMPL.JointCount; jointIndex++) {
            for (int shapeBetaIndex = 0; shapeBetaIndex < SMPL.ShapeBetaCount; shapeBetaIndex++) {
                // dimension refers to the separate x,y,z values.
                for (int vector3Dimension = 0; vector3Dimension < SMPL.DimensionsOfAVector3; vector3Dimension++) {
                    Matrix jointRegressorMatrix = jointsRegressor[vector3Dimension];
                    jointRegressorMatrix[jointIndex, shapeBetaIndex] =
                        betasJointRegressorNode[jointIndex][vector3Dimension][shapeBetaIndex].AsDouble;
                }
            }
        }
    }

    void ParseTemplatesFromJSON(JSONNode node) {
        JSONNode templateNode = node[SMPL.JSONKeys.JointTemplates];
        for (int dimensionOfVector3 = 0; dimensionOfVector3 < SMPL.DimensionsOfAVector3; dimensionOfVector3++) {
            template[dimensionOfVector3] = new Matrix(SMPL.JointCount, 1);
        }

        for (int jointIndex = 0; jointIndex < SMPL.JointCount; jointIndex++) {
            for (int vector3Dimension = 0; vector3Dimension < SMPL.DimensionsOfAVector3; vector3Dimension++) {
                double x = templateNode[jointIndex][vector3Dimension].AsDouble;
                Matrix templateMatrix = template[vector3Dimension];
                templateMatrix[jointIndex, 0] = x;
            }
        }
    }

    public JointCalculator Build() {
        return new JointCalculator(template, jointsRegressor);
    }
}