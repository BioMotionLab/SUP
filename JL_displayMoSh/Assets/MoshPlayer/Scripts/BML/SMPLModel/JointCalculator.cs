using MoshPlayer.Scripts.ThirdParty.Matrix;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class JointCalculator {
        const int FirstMatrixColumn = 0;

        readonly Matrix[] template;
        readonly Matrix[] jointsRegressor;
        
        
        public JointCalculator(Matrix[] template, Matrix[] jointsRegressor) {
            this.template = template;
            this.jointsRegressor = jointsRegressor;
        }

        /// <summary>
        /// Calculate initial joint positions from rawBodyShapeWeightBetas. 
        /// </summary>
        /// <returns>The joints.</returns>
        public Vector3[] CalculateJointPositions(float[] betas) {
            Matrix[] newRegressedJoints = RunJointsThroughRegressor(betas);
            Vector3[] updatedJointPositions = ConvertJointMatrixToArray(newRegressedJoints);
            return updatedJointPositions;
        }

        Vector3[] ConvertJointMatrixToArray(Matrix[] newRegressedJoints) {
            Vector3[] updatedJointArray = new Vector3[SMPLConstants.JointCount];
            for (int jointIndex = 0; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                Vector3 jointVectorInMayaCoords = RetrieveJointsFromMatrixAsVector3(newRegressedJoints, jointIndex);
                Vector3 finalJointVector = jointVectorInMayaCoords.ToLeftHanded();
                updatedJointArray[jointIndex] = finalJointVector;
            }
            return updatedJointArray;
        }

        Matrix[] RunJointsThroughRegressor(float[] betas) {
            Matrix betaMatrix = CopyBetaArrayToMatrix(betas);
            Matrix[] newJoints = new Matrix[SMPLConstants.DimensionsOfAVector3]; //stores x,y,z joint values as dimensions

            for (int vector3Dimension = 0; vector3Dimension < SMPLConstants.DimensionsOfAVector3; vector3Dimension++) {
                newJoints[vector3Dimension] = jointsRegressor[vector3Dimension] * betaMatrix + template[vector3Dimension];
            }

            return newJoints;
        }

        /// <summary>
        /// Converts multidimensional array of joints into xyz 
        /// </summary>
        /// <param name="newJoints"></param>
        /// <param name="jointIndex"></param>
        /// <returns></returns>
        static Vector3 RetrieveJointsFromMatrixAsVector3(Matrix[] newJoints, int jointIndex) {
            const int xDimension = 0;
            const int yDimension = 1;
            const int zDimension = 2;
        
            //Cast to float is needed because Matrices are dumb.
            return new Vector3((float)newJoints[xDimension][jointIndex, FirstMatrixColumn],
                               (float)newJoints[yDimension][jointIndex, FirstMatrixColumn],
                               (float)newJoints[zDimension][jointIndex, FirstMatrixColumn]);
        }

        /// <summary>
        /// Copies the rawBodyShapeWeightBetas array to a matrix with one column for use in matrix multiplication.
        /// </summary>
        /// <param name="betas"></param>
        /// <returns></returns>
        static Matrix CopyBetaArrayToMatrix(float[] betas) {
            Matrix betaMatrix = new Matrix(SMPLConstants.BodyShapeBetaCount, 1); // column vector.
            for (int betaIndex = 0; betaIndex < SMPLConstants.BodyShapeBetaCount; betaIndex++) {
                betaMatrix[betaIndex, FirstMatrixColumn] = betas[betaIndex];
            }
            return betaMatrix;
        }

        public Vector3[] CalculateJointsAtZeroedBetas() {
            float[] zeroedBetas = new float[SMPLConstants.BodyShapeBetaCount];
            return CalculateJointPositions(zeroedBetas);
        }
    }
}