using System;
using LightweightMatrixCSharp;
using UnityEngine;

public class JointCalculator {
    const int FirstMatrixColumn = 0;

    readonly Matrix[] template;
    readonly Matrix[] jointsRegressor; // doesn't need reset. 
    
    // private constructor. 
    public JointCalculator(Matrix[] template, Matrix[] jointsRegressor) {
        this.template = template;
        this.jointsRegressor = jointsRegressor;
    }

    /// <summary>
    /// Calculate initial joint positions from betas. 
    /// </summary>
    /// <returns>The joints.</returns>
    public Vector3[] CalculateJointPositions(float[] betas) {
        Matrix[] newRegressedJoints = RunJointsThroughRegressor(betas);
        Vector3[] updatedJoints = ConvertJointMatrixToArray(newRegressedJoints);
        return updatedJoints;
    }

    Vector3[] ConvertJointMatrixToArray(Matrix[] newRegressedJoints) {
        Vector3[] updatedJointArray = new Vector3[SMPL.JointCount];
        for (int jointIndex = 0; jointIndex < SMPL.JointCount; jointIndex++) {
            Vector3 jointVectorInMayaCoords = RetrieveJointsFromMatrixAsVector3(newRegressedJoints, jointIndex);
            Vector3 finalJointVector = ConvertFromMayaToUnityCoordinateSystem(jointVectorInMayaCoords);
            updatedJointArray[jointIndex] = finalJointVector;
        }
        return updatedJointArray;
    }

    Matrix[] RunJointsThroughRegressor(float[] betas) {
        Matrix betaMatrix = CopyBetaArrayToVector(betas);
        Matrix[] newJoints = new Matrix[SMPL.DimensionsOfAVector3]; //stores x,y,z joint values as dimensions

        for (int vector3Dimension = 0; vector3Dimension < SMPL.DimensionsOfAVector3; vector3Dimension++) {
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
        
        //Cast to float is needed since matrix is a Double value.
        return new Vector3((float)newJoints[xDimension][jointIndex, FirstMatrixColumn],
                           (float)newJoints[yDimension][jointIndex, FirstMatrixColumn],
                           (float)newJoints[zDimension][jointIndex, FirstMatrixColumn]);
    }

    /// <summary>
    /// Copies the betas array to a matrix with one column for use in matrix multiplication.
    /// </summary>
    /// <param name="betas"></param>
    /// <returns></returns>
    static Matrix CopyBetaArrayToVector(float[] betas) {
        Matrix betaMatrix = new Matrix(SMPL.ShapeBetaCount, 1); // column vector.
        for (int betaIndex = 0; betaIndex < SMPL.ShapeBetaCount; betaIndex++) {
            betaMatrix[betaIndex, FirstMatrixColumn] = betas[betaIndex];
        }
        return betaMatrix;
    }

    static Vector3 ConvertFromMayaToUnityCoordinateSystem(Vector3 inMayaCoordinates) {
        Vector3 inUnityCoordinates = new Vector3(-inMayaCoordinates.x,
                                                 inMayaCoordinates.y,
                                                 inMayaCoordinates.z);
        return inUnityCoordinates;
    }

    public Vector3[] CalculateJointsAtZeroedBetas() {
        float[] zeroedBetas = new float[SMPL.ShapeBetaCount];
        return CalculateJointPositions(zeroedBetas);
    }
}