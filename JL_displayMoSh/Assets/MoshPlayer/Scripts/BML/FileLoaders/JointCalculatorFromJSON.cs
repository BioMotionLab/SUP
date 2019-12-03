using MoshPlayer.Scripts.ThirdParty.Matrix;
using MoshPlayer.Scripts.ThirdParty.SimpleJSON;
using UnityEngine;

namespace MoshPlayer.Scripts.BML {
    public class JointCalculatorFromJSON {
    
        readonly Matrix[] jointsRegressor;
        readonly Matrix[] template;

        public JointCalculatorFromJSON(TextAsset jsonText) {
            template = new Matrix [SMPLConstants.DimensionsOfAVector3];
            jointsRegressor = new Matrix [SMPLConstants.DimensionsOfAVector3];

            JSONNode mainNode = JSON.Parse(jsonText.text);
        
            ParseTemplatesFromJSON(mainNode);
            ParseJointRegressorsFromJSON(mainNode);
        }
    
        void ParseJointRegressorsFromJSON(JSONNode node) {
            JSONNode betasJointRegressorNode = node[SMPLConstants.JSONKeys.BetaJointRegressors];
        
            for (int vector3Dimension = 0; vector3Dimension < SMPLConstants.DimensionsOfAVector3; vector3Dimension++) {
                jointsRegressor[vector3Dimension] = new Matrix(SMPLConstants.JointCount, SMPLConstants.ShapeBetaCount);
            }

            for (int jointIndex = 0; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                for (int shapeBetaIndex = 0; shapeBetaIndex < SMPLConstants.ShapeBetaCount; shapeBetaIndex++) {
                    // dimension refers to the separate x,y,z values.
                    for (int vector3Dimension = 0; vector3Dimension < SMPLConstants.DimensionsOfAVector3; vector3Dimension++) {
                        Matrix jointRegressorMatrix = jointsRegressor[vector3Dimension];
                        jointRegressorMatrix[jointIndex, shapeBetaIndex] =
                            betasJointRegressorNode[jointIndex][vector3Dimension][shapeBetaIndex].AsDouble;
                    }
                }
            }
        }

        void ParseTemplatesFromJSON(JSONNode node) {
            JSONNode templateNode = node[SMPLConstants.JSONKeys.JointTemplates];
            for (int dimensionOfVector3 = 0; dimensionOfVector3 < SMPLConstants.DimensionsOfAVector3; dimensionOfVector3++) {
                template[dimensionOfVector3] = new Matrix(SMPLConstants.JointCount, 1);
            }

            for (int jointIndex = 0; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                for (int vector3Dimension = 0; vector3Dimension < SMPLConstants.DimensionsOfAVector3; vector3Dimension++) {
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
}