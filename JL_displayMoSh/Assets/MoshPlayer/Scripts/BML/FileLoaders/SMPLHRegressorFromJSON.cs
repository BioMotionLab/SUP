using System;
using MoshPlayer.Scripts.ThirdParty.SimpleJSON;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MoshPlayer.Scripts.BML.FileLoaders {
    /// <summary>
    /// Reads the regressor JSON file and loads it into Unity-Compatible formats.
    /// File should contain a Joint template and Joint regressor to fit formula
    /// JointLocations = JointTemplate + JointRegressor * Betas
    /// I wrote a pythong script included in the github repository to extract the necessary matricies
    /// from the model.npz files provided by MPI
    /// </summary>
    public class SMPLHRegressorFromJSON {
        const string JointRegressorJSONKey = "joint_regressor";
        const string JointTemplateJSONKey  = "joint_template";

        const int SMPLHJointCount = 52;
        const int SMPLHBetaCount  = 16;

        const int ThreeDimensions = 3;
        const int XDimension      = 0;
        const int YDimension      = 1;
        const int ZDimension      = 2;


        Matrix<double> jointRegressorMatrixX;
        Matrix<double> jointRegressorMatrixY;
        Matrix<double> jointRegressorMatrixZ;
        Matrix<double> jointTemplate;


        /// <summary>
        /// Static builder to create a Regressor from a JSON file.
        /// </summary>
        /// <param animationName="jsonFile">The regressor JSON File</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static JointRegressor LoadRegressorFromJSON(TextAsset jsonFile) {
            if (jsonFile == null) throw new NullReferenceException("No JointRegressor file defined");

            JSONNode jsonNode = JSON.Parse(jsonFile.text);

            SMPLHRegressorFromJSON fromJSON = new SMPLHRegressorFromJSON();
            fromJSON.LoadJointTemplate(jsonNode);
            fromJSON.LoadJointRegressor(jsonNode);

            JointRegressor jointRegressor = new JointRegressor(fromJSON.jointTemplate,
                                                               fromJSON.jointRegressorMatrixX,
                                                               fromJSON.jointRegressorMatrixY,
                                                               fromJSON.jointRegressorMatrixZ);
            return jointRegressor;
        }

        /// <summary>
        /// Loads the Joint template from a JSON node
        /// </summary>
        /// <param animationName="templateJSONNode"></param>
        void LoadJointTemplate(JSONNode templateJSONNode) {
            JSONNode jointTemplateNode = templateJSONNode[JointTemplateJSONKey];


            double[,] jointTemplateArray = new double[SMPLHJointCount, ThreeDimensions];
            for (int jointIndex = 0; jointIndex < SMPLHJointCount; jointIndex++) {
                for (int dimensionIndex = 0; dimensionIndex < ThreeDimensions; dimensionIndex++) {
                    jointTemplateArray[jointIndex, dimensionIndex] = jointTemplateNode[jointIndex][dimensionIndex];
                }
            }

            jointTemplate = DenseMatrix.OfArray(jointTemplateArray);
        }

        /// <summary>
        /// Loads the joint regressor into Unity-compatible matrix format (3 separate matrices, one for each dimension)
        /// </summary>
        /// <param animationName="regressorJSONNode"></param>
        void LoadJointRegressor(JSONNode regressorJSONNode) {
            JSONNode regressorNode = regressorJSONNode[JointRegressorJSONKey];

            //joint jointRegressor stored as 52x3x16 jointTemplate, need to decompose into 3 separate 52x16 matricies.
            double[,] jointRegressorX = new double[SMPLHJointCount, SMPLHBetaCount];
            double[,] jointRegressorY = new double[SMPLHJointCount, SMPLHBetaCount];
            double[,] jointRegressorZ = new double[SMPLHJointCount, SMPLHBetaCount];

            for (int jointIndex = 0; jointIndex < SMPLHJointCount; jointIndex++) {
                for (int betaIndex = 0; betaIndex < SMPLHBetaCount; betaIndex++) {
                    jointRegressorX[jointIndex, betaIndex] = regressorNode[jointIndex][XDimension][betaIndex];
                    jointRegressorY[jointIndex, betaIndex] = regressorNode[jointIndex][YDimension][betaIndex];
                    jointRegressorZ[jointIndex, betaIndex] = regressorNode[jointIndex][ZDimension][betaIndex];
                }
            }

            jointRegressorMatrixX = DenseMatrix.OfArray(jointRegressorX);
            jointRegressorMatrixY = DenseMatrix.OfArray(jointRegressorY);
            jointRegressorMatrixZ = DenseMatrix.OfArray(jointRegressorZ);
        }
    }
}