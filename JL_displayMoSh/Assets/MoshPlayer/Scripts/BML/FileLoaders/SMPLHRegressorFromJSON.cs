

using System;
using System.Collections.Generic;
using System.Text;
using MoshPlayer.Scripts.ThirdParty.SimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MoshPlayer.Scripts.BML.FileLoaders {
    public static class SMPLHRegressorFromJSON {
        
        public static Regressor LoadRegressorFromJSON(TextAsset jsonFileAsString) {
            
            if (jsonFileAsString == null) throw new NullReferenceException("No Regressor file defined in Model");

            JSONNode jsonNode = JSON.Parse(jsonFileAsString.text);
            
            JSONNode jointTemplateNode = jsonNode["joint_template"];

            
            double[,] jointTemplate = new double[52,3];
            for (int jointIndex = 0; jointIndex < 52; jointIndex++) {
                for (int dimensionIndex = 0; dimensionIndex < 3; dimensionIndex++) {
                    jointTemplate[jointIndex, dimensionIndex] = jointTemplateNode[jointIndex][dimensionIndex];
                }
            }

            Matrix<double> jointTemplateMatrix = DenseMatrix.OfArray(jointTemplate);
            
            //Debug.Log($"{jointTemplate.rows} {jointTemplate.cols}");

            JSONNode regressorNode = jsonNode["joint_regressor"];
            
            //joint regressor stored as 52x3x16 jointTemplate, need to decompose into 3 52x16 matricies.
            double[,] jointRegressorX = new double[52,16];
            double[,] jointRegressorY = new double[52,16];
            double[,] jointRegressorZ = new double[52,16];
            
            for (int jointIndex = 0; jointIndex < 52; jointIndex++) {
                for (int betaIndex = 0; betaIndex < 16; betaIndex++) {
                    jointRegressorX[jointIndex, betaIndex] = regressorNode[jointIndex][0][betaIndex];
                    jointRegressorY[jointIndex, betaIndex] = regressorNode[jointIndex][1][betaIndex];
                    jointRegressorZ[jointIndex, betaIndex] = regressorNode[jointIndex][2][betaIndex];
                }
            }

            Matrix<double> jointRegressorMatrixX = DenseMatrix.OfArray(jointRegressorX);
            Matrix<double> jointRegressorMatrixY = DenseMatrix.OfArray(jointRegressorY);
            Matrix<double> jointRegressorMatrixZ = DenseMatrix.OfArray(jointRegressorZ);
            //Debug.Log($"{jointRegressor[0].rows} {jointRegressor.Length} {jointRegressor[0].cols} ");
            //Debug.Log($"{joint.cols},{joint.rows}");
            
            Regressor regressor = new Regressor(jointTemplateMatrix, jointRegressorMatrixX, jointRegressorMatrixY, jointRegressorMatrixZ);

            return regressor;
            

        }
    }

    public class Regressor {
        readonly Matrix<double> jointTemplate;
        readonly Matrix<double> jointRegressorMatrixX;
        readonly Matrix<double> jointRegressorMatrixY;
        readonly Matrix<double> jointRegressorMatrixZ;

        public Regressor(Matrix<double> jointTemplate, Matrix<double> jointRegressorMatrixX, Matrix<double> jointRegressorMatrixY, Matrix<double> jointRegressorMatrixZ) {
            this.jointTemplate = jointTemplate;
            this.jointRegressorMatrixX = jointRegressorMatrixX;
            this.jointRegressorMatrixY = jointRegressorMatrixY;
            this.jointRegressorMatrixZ = jointRegressorMatrixZ;
        }

        public Vector3[] JointPositionFrom(float[] betaArray) {
            
            StringBuilder debugOutput = new StringBuilder($"Betas input to regressor:\n");
            foreach (float beta in betaArray) {
                debugOutput.AppendLine(beta.ToString("f6"));
            }
            Debug.Log(debugOutput);
            
            MatrixBuilder<double> M = Matrix<double>.Build;
            VectorBuilder<double> V = Vector<double>.Build;
            
            double[,] betaMatrix = new double[16,1];
            for (int i = 0; i < betaArray.Length; i++) {
                betaMatrix[i,0] = betaArray[i];
            }
            
            Matrix<double> betas = DenseMatrix.OfArray(betaMatrix);
            
            
            Matrix<double> dimensionResultsX = jointRegressorMatrixX.Multiply(betas);
            Matrix<double> dimensionResultsY = jointRegressorMatrixY.Multiply(betas);
            Matrix<double> dimensionResultsZ = jointRegressorMatrixZ.Multiply(betas);

            Vector<double>[] combinedColumns = new Vector<double>[] {dimensionResultsX.Column(0), 
                                                                        dimensionResultsY.Column(0), 
                                                                        dimensionResultsZ.Column(0)};
            Matrix<double> result = DenseMatrix.OfColumns(combinedColumns);
            
            
            //Debug.Log($"result shape: {result.RowCount}, {result.ColumnCount}");
            
            var addedTemplate = jointTemplate + result;
            
            Vector3[] positions = GatherPositionsFromMatrix(addedTemplate);
            
            return positions;

        }

        static Vector3[] GatherPositionsFromMatrix(Matrix<double> addedTemplate) {
            Vector3[] positions = new Vector3[52];
            StringBuilder debugOuput = new StringBuilder("Joint positions from regressed: \n");
            for (int jointIndex = 0; jointIndex < 52; jointIndex++) {
                Vector3 newPosition = new Vector3();
                newPosition.x = (float) addedTemplate[jointIndex, 0];
                newPosition.y = (float) addedTemplate[jointIndex, 1];
                newPosition.z = (float) addedTemplate[jointIndex, 2];
                positions[jointIndex] = newPosition;
                debugOuput.AppendLine(newPosition.ToString("f5"));
            }

            Debug.Log($"{debugOuput}");
            return positions;
        }
    }
}
