/*

License:
--------
Copyright 2017 Naureen Mahmood and the Max Planck Gesellschaft.  
All rights reserved. This software is provided for research purposes only.
By using this software you agree to the terms of the SMPL Model license here http://smpl.is.tue.mpg.de/license

To get more information about SMPL and other downloads visit: http://smpl.is.tue.mpg.
For comments or questions, please email us at: smpl@tuebingen.mpg.de

Special thanks to Joachim Tesch and Max Planck Institute for Biological Cybernetics 
in helping to create and test these scripts for Unity.

This is a demo version of the scripts & sample project for using the SMPL model's shape-blendshapes 
& corrective pose-blendshapes inside Unity. We would be happy to receive comments, help and suggestions 
on improving the model and in making it available on more platforms. 

	About this Script:
	=================
	This script file defines the SMPLJointCalculator class which computes the joints of the SMPL model 
	whenever the base mesh is changed using 'shapeParmsJSON' field in the SMPLBlendshapes module.

	The class uses the joint-regressor from one for the 'SMPL_jReg_*.json' file to regress to the 
	updated joint locations for the new mesh shape.
*/

using System;
using LightweightMatrixCSharp;
using SimpleJSON;
using UnityEngine;

namespace SMPL.Scripts.mpi {
	
	public class SMPLJointCalculator {
		
		const string BetasRegressorJSONKey = "betasJ_regr";
		
		readonly int       numberOfJoints;
		readonly int       numberOfBetas;
		Matrix[]  jointTemplate;
		Matrix[]  regressor;
		public Vector3[] Joints;

		public SMPLJointCalculator(TextAsset JSONFile, int numberOfJoints, int numberOfBetas)
		{
			if (JSONFile == null)
				throw new System.ArgumentNullException("ERROR: no joint regressor JSON file provided");
			
			this.numberOfBetas = numberOfBetas;
			this.numberOfJoints = numberOfJoints;

			string JSONText = JSONFile.text;
			JSONNode loadedJSON = JSON.Parse(JSONText);

			SetUpJointMatrices(loadedJSON);
		}

		void SetUpJointMatrices(JSONNode node) {
			Joints = new Vector3[numberOfJoints];
			jointTemplate = new Matrix[3];
			regressor = new Matrix[3];

			for (int i = 0; i <= 2; i++) {
				jointTemplate[i] = new Matrix(numberOfJoints, 1);
				regressor[i] = new Matrix(numberOfJoints, numberOfBetas);
			}

			// Init matrices
			for (int i = 0; i < numberOfJoints; i++) {
				// Init joint template matrix
				double x = node["template_J"][i][0].AsDouble;
				double y = node["template_J"][i][1].AsDouble;
				double z = node["template_J"][i][2].AsDouble;

				(jointTemplate[0])[i, 0] = x;
				(jointTemplate[1])[i, 0] = y;
				(jointTemplate[2])[i, 0] = z;

				// Init beta regressor matrix    
				InitBetaRegressorMatrix(numberOfBetas, i, node);
			}
		}


		void InitBetaRegressorMatrix(int numberOfBetas, int i, JSONNode node) {
			for (int j = 0; j < numberOfBetas; j++) {
				(regressor[0])[i, j] = node[BetasRegressorJSONKey][i][0][j].AsDouble;
				(regressor[1])[i, j] = node[BetasRegressorJSONKey][i][1][j].AsDouble;
				(regressor[2])[i, j] = node[BetasRegressorJSONKey][i][2][j].AsDouble;
			}
		}

		public void UpdateJointPositions(float[] betas)
		{
			if (betas.Length != numberOfBetas) {
				Debug.LogError("ERROR: Invalid beta input value count in baked mesh: need " + numberOfBetas + " but have " + betas.Length);
				return;
			}

			Matrix betaMatrix = CreateBetaMatrix(betas);

			// Apply joint regressor to beta matrix to calculate new joint positions
			Matrix newJointsX = regressor[0] * betaMatrix + jointTemplate[0];
			Matrix newJointsY = regressor[1] * betaMatrix + jointTemplate[1];
			Matrix newJointsZ = regressor[2] * betaMatrix + jointTemplate[2];

			// Update joints vector
			for (int row = 0; row < numberOfJoints; row++) {
				var rawJointVector = new Vector3((float)newJointsX[row, 0], (float)newJointsY[row, 0], (float)newJointsZ[row, 0]);
				var jointVector = rawJointVector.ToLeftHandedCoordinateSystem();
				Joints[row] = jointVector;
			}
		}

		Matrix CreateBetaMatrix(float[] betas) {
			Matrix betaMatrix = new Matrix(numberOfBetas, 1);
			for (int row = 0; row < numberOfBetas; row++) {
				betaMatrix[row, 0] = betas[row];
				//Debug.Log("beta " + row + ": " + betas[row]);
			}
			return betaMatrix;
		}
	}
}
