using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.FileLoaders {
    /// <summary>
    /// This Class takes a set of an individual body's shape-betas and
    /// calculates the correct skeleton for that body.
    ///
    /// It depends on a joint template combined with a
    /// "Regressor" matrix, which is a slightly converted version of J_template in the MPI model's documentation.
    /// Instead of calculating the joint locations from the vertices, as described by MPI in their python code:
    ///     J_locations = J_Regressor * shaped_verticies
    /// 
    /// Here, we can't read vertices directly because Unity does not keep consistent vertex number nor vertex index.
    /// So we pre-computed J_template from the verticies using a python script,
    /// which is saved then read from a JSON file. So we use the formula:
    ///
    ///     J_locations = J_template + JRegressor * Betas
    ///
    /// where J_template is dimensions [NumJoints, 3] (Avg mesh joint locations)
    ///      JRegressor is dimensions [NumJoints, 3, NumBetas]
    ///      betas is dimensions [NumBetas,1]
    /// </summary>
    public class JointRegressor {
        readonly Matrix<double>  jointTemplate;
        readonly Matrix<double>  jointRegressorMatrixX;
        readonly Matrix<double>  jointRegressorMatrixY;
        readonly Matrix<double>  jointRegressorMatrixZ;

        public JointRegressor(Matrix<double> jointTemplate,         Matrix<double> jointRegressorMatrixX,
                              Matrix<double> jointRegressorMatrixY, Matrix<double> jointRegressorMatrixZ) {
            this.jointTemplate = jointTemplate;
            this.jointRegressorMatrixX = jointRegressorMatrixX;
            this.jointRegressorMatrixY = jointRegressorMatrixY;
            this.jointRegressorMatrixZ = jointRegressorMatrixZ;
        }

        public Vector3[] JointPositionFrom(ModelDefinition model, float[] betasArray) {
            Matrix<double> betas = ConvertBetasToMatrix(betasArray);

            Matrix<double> finalNewPositionMatrix = jointTemplate + J_RegressorDotBetas(betas);
            
            Vector3[] positionsInMayaCoords = MatrixToPositionArray(model, finalNewPositionMatrix);
            Vector3[] positions = ConvertToUnityCoordinateSystem(positionsInMayaCoords);
            return positions;
        }
        
        Vector3[] ConvertToUnityCoordinateSystem(Vector3[] jointPositions) {
            Vector3[] flippedJointPositions = new Vector3[jointPositions.Length];
            for (int index = 0; index < jointPositions.Length; index++) {
                Vector3 jointPosition = jointPositions[index];
                flippedJointPositions[index] = new Vector3(-jointPosition.x, jointPosition.y, jointPosition.z);
            }

            return flippedJointPositions;
        }


        /// <summary>
        /// Since C# lacks "einsum" function, have to do complex matrix multiplication manually
        ///
        /// this replaces python the formula:
        /// result = np.einsum('ijk,kl->ij', J_regressor, betas)
        /// 
        /// </summary>
        /// <param animationName="betas"></param>
        /// <returns></returns>
        Matrix<double> J_RegressorDotBetas(Matrix<double> betas) {
            Matrix<double> dimensionResultsX = jointRegressorMatrixX.Multiply(betas);
            Matrix<double> dimensionResultsY = jointRegressorMatrixY.Multiply(betas);
            Matrix<double> dimensionResultsZ = jointRegressorMatrixZ.Multiply(betas);
            Matrix<double> multiplicationResult = StackDimensionsBackToOneMatrix(
                                                                                 dimensionResultsX,
                                                                                 dimensionResultsY,
                                                                                 dimensionResultsZ);
            return multiplicationResult;
        }


        /// <summary>
        /// Since C# lacks good matrix manipulation, have to stack manually.
        /// Basically just converts 3 separate XYZ matricies (from multiplication)
        /// back in to a 3-dimensional matrix
        /// </summary>
        /// <param animationName="dimensionResultsX"></param>
        /// <param animationName="dimensionResultsY"></param>
        /// <param animationName="dimensionResultsZ"></param>
        /// <returns></returns>
        static Matrix<double> StackDimensionsBackToOneMatrix(Matrix<double> dimensionResultsX,
                                                             Matrix<double> dimensionResultsY,
                                                             Matrix<double> dimensionResultsZ
        ) {
            Vector<double>[] columnsToStack = {
                                                  dimensionResultsX.Column(0),
                                                  dimensionResultsY.Column(0),
                                                  dimensionResultsZ.Column(0)
                                              };

            Matrix<double> stackedColumns = DenseMatrix.OfColumns(columnsToStack);
            return stackedColumns;
        }

        /// <summary>
        /// Converts array of betas to 
        /// </summary>
        /// <param animationName="betaArray"></param>
        /// <returns></returns>
        static Matrix<double> ConvertBetasToMatrix(float[] betaArray) {
            double[,] betaMatrix = new double[betaArray.Length, 1];
            for (int i = 0; i < betaArray.Length; i++) {
                betaMatrix[i, 0] = betaArray[i];
            }

            Matrix<double> betas = DenseMatrix.OfArray(betaMatrix);
            return betas;
        }


        /// <summary>
        /// Converts Matrix Back to array of Vector3 positions
        /// </summary>
        /// <param animationName="model"></param>
        /// <param animationName="positionMatrix"></param>
        /// <returns></returns>
        static Vector3[] MatrixToPositionArray(ModelDefinition model, Matrix<double> positionMatrix) {
            Vector3[] positions = new Vector3[model.JointCount];

            for (int jointIndex = 0; jointIndex < model.JointCount; jointIndex++) {
                Vector3 newPosition = new Vector3 {
                                                      x = (float) positionMatrix[jointIndex, 0],
                                                      y = (float) positionMatrix[jointIndex, 1],
                                                      z = (float) positionMatrix[jointIndex, 2]
                                                  };

                positions[jointIndex] = newPosition;
            }

            return positions;
        }
    }
}