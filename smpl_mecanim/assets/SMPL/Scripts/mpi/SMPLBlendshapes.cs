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
	==================
	This script file defines the SMPLBlendshapes class which creates the module to be attached to the 
	SMPL mesh inside the Unity scene. This script controls both the 'Shape blendshapes' and 'Pose blendshapes'
	for the SMPL model. 

	To use this module:
	1. Please attach this class as a Component to the Mesh object of the SMPL model in the scene (make 
	sure it is attached to the 'Mesh' object, and not the main 'Transform' object. 

	2. Link the SMPL_jReg_*.json file corresponding to the gender of the model to the 'Joint Regressor JSON'
	field of the component (i.e. SMPL_jReg_f_*.json for female, SMPL_jReg_m_*.json for male)

	(optional)
	3. If you want to try using a new shape, link one of the SMPL_samplShape*.json file to the 'Shape Parms JSON'
	field of the component (you can create your own shape parameters similar to the SMPL_sampleShape*.json 
	files)

	(optional)
	4. If the model is running slow on your machine, you can speed up the runtime by turning on the 'Optimize 
	Pose Blends' switch. This makes it so that the script only sets the first 40 most important 
	Pose Blendshapes (out of a total of 207), and ignores the rest to speed up the runtime. 

	This script will create the new body shape provided by the shape parameters in the 'Shape Parms JSON' field. 
	The script updates the Shape blendshapes to create the new mesh to match the shape parameters from the provided 
	JSON file, and updates the skeleton inside the model to match the new body shape. 

	This script will also apply the pose-dependent deformations. These are parameters that are updated with each new 
	pose to create more realistic joint creases and muscle-bulging effects based on the pose. This scripts applies 
	these deformations by updating the Pose blendshapes in the model at each new pose. 
	
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SMPL.Scripts.mpi {
	public class SMPLBlendshapes : MonoBehaviour {
	
		const string GenderJSONKey = "gender";
	
		const int   NumberOfPoseBlendshapesToSet   = 40;
		const float BlendshapeScaleFactor          = 5.0f;
		const int   NumberOfJoints                 = 24;
		const int   NumberOfShapeParameters        = 10;
		const int   NumberOfShapeParametersDoubled = NumberOfShapeParameters * 2;
		
		[SerializeField] 
		TextAsset JointRegressorJson = default;
		
		[SerializeField] 
		TextAsset ShapeParametersJson = default;
	
		[SerializeField] 
		bool OptimizePoseBlends = false;

		SkinnedMeshRenderer   skinnedMeshRenderer;
		SMPLJointCalculator   jointCalculator;
		SMPLBoneModifier      boneModifier;
		SMPLOptimalPoseBlends optimalPoseBlends;

		Gender    gender;
		List<int> poseBlendshapesToSet;
	
		float[] shapeParameters;

		void Awake()
		{
			skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer> ();
		}
	
		void OnApplicationQuit() {
			CleanupAndResetAll();
		}

		void CleanupAndResetAll() {
			ResetShapeParametersToZero();
			SetShapeBlendValues();
			UpdatePoseBlendshapeValues();
			UpdateBonePositions();

			boneModifier.RestoreOriginalBindPose();
		}

	
		void Start () 
		{
			GetGender();
			InitializeShapeParametersToZero();
			InitializeJointsModifier();
			InitializeJointsCalculator();
			OptimizePoseBlendshapes();
			ReadBetasFromFileIfGiven();
		}

		void ReadBetasFromFileIfGiven() {
			if (ShapeParametersJson != null) ReadShapeParametersFromJSON();
		}

		void OptimizePoseBlendshapes() {
			optimalPoseBlends = new SMPLOptimalPoseBlends(gender, NumberOfPoseBlendshapesToSet);
			poseBlendshapesToSet = OptimizePoseBlends ? 
				new List<int>(optimalPoseBlends.SelectedBlendshapes) : 
				null;
		}

		void InitializeJointsCalculator() {
			jointCalculator = new SMPLJointCalculator(JointRegressorJson, NumberOfJoints, shapeParameters.Length);
		
		}

		void InitializeJointsModifier() {
			boneModifier = new SMPLBoneModifier(skinnedMeshRenderer);
		
		}

		void InitializeShapeParametersToZero() {
			shapeParameters = new float[NumberOfShapeParameters];
			ResetShapeParametersToZero();
		}
	
		void Update()
		{
			UpdatePoseBlendshapeValues();
		}
	
		void UpdatePoseBlendshapeValues() {
			Transform[] bones = boneModifier.Bones;
		
			string boneNamePrefix = boneModifier.BoneNamePrefix;
		
			foreach (Transform bone in bones) {
			
				string boneName = bone.name;
				boneName = RemoveBoneNamePrefix(boneName, boneNamePrefix);

				if (boneName == "root" || boneName == "Pelvis")
					continue;

				// Convert quaternion from Unity's LHS to RHS because SMPL model's
				// pose-blendshapes were trained using a RHS coordinate system
				Quaternion boneLocalRotationRightHanded = bone.localRotation.ToRightHanded();
			
				float[] rot3X3 = boneLocalRotationRightHanded.To3X3Matrix();
			
				int boneIndex = boneModifier.GetJointIndexFromBoneName(boneName);
				int doubledIndex = boneIndex * 9 * 2; 
			
				SetBlendShapeWeights(rot3X3, doubledIndex);
			
			}
		}

		/// <summary>
		/// Remove f_avg/m_avg prefix from bone name
		/// </summary>
		/// <param name="boneName"></param>
		/// <param name="boneNamePrefix"></param>
		/// <returns></returns>
		static string RemoveBoneNamePrefix(string boneName, string boneNamePrefix) {
			boneName = boneName.Replace(boneNamePrefix, "");
			return boneName;
		}

		void SetBlendShapeWeights(float[] rot3X3, int doubledIndex) {
		
			//TODO repetition below in SetBlendShapeValues, perhaps refactor to join?
		
			for (int indexIndRotationMatrix = 0; indexIndRotationMatrix < rot3X3.Length; indexIndRotationMatrix++) {
				float positive, negative;
				float theta = rot3X3[indexIndRotationMatrix];

				if (theta >= 0) {
					positive = theta;
					negative = 0.0f;
				}
				else {
					positive = 0.0f;
					negative = -theta;
				}

				int index = doubledIndex * 9 + indexIndRotationMatrix;

				if (OptimizePoseBlends && !poseBlendshapesToSet.Contains(index)) continue;

				int positiveBlendshapeIndex = NumberOfShapeParametersDoubled + doubledIndex + (indexIndRotationMatrix * 2) + 0;
				int negativeBlendshapeIndex = NumberOfShapeParametersDoubled + doubledIndex + (indexIndRotationMatrix * 2) + 1;
				skinnedMeshRenderer.SetBlendShapeWeight(positiveBlendshapeIndex, positive * 100.0f);
				skinnedMeshRenderer.SetBlendShapeWeight(negativeBlendshapeIndex, negative * 100.0f);
			}
		}


		public void ResetShapeParametersToZero()
		{
			for (int bi = 0; bi < NumberOfShapeParameters; bi++) 
				shapeParameters[bi] = 0.0f;
		}
	
		/// <summary>
		/// Sets shape parameters (betas) of avg mesh in FBX model to shape-parameters (betas)
		/// </summary>
		public void SetShapeBlendValues()
		{
			for (int i=0; i < NumberOfShapeParameters; i++)
			{
				float positiveBeta, negativeBeta;
				float beta = shapeParameters[i] / BlendshapeScaleFactor;

				if (beta >= 0) 
				{
					positiveBeta = beta;
					negativeBeta = 0.0f;
				}
				else 
				{
					positiveBeta = 0.0f;
					negativeBeta = -beta;
				}

				int positiveBlendShapeIndex = i * 2 + 0;
				skinnedMeshRenderer.SetBlendShapeWeight(positiveBlendShapeIndex, positiveBeta * 100.0f); // map [0, 1] space to [0, 100]
				int negativeBlendShapeIndex = i * 2 + 1;
				skinnedMeshRenderer.SetBlendShapeWeight(negativeBlendShapeIndex, negativeBeta * 100.0f); // map [0, 1] space to [0, 100]
			}

		}
	
		public void UpdateBonePositions()
		{
			jointCalculator.UpdateJointPositions(shapeParameters);
			Vector3[] joints = jointCalculator.Joints;
			boneModifier.updateBonePositions(joints);
		}
	
	
		/// <summary>
		/// Load shape parameters, aka 'betas', from the JSON file provided.
		/// These parameters change the body shape of the model according to
		/// the shape-parametrization defined in the SMPL model paper.
		/// </summary>
		void ReadShapeParametersFromJSON()
		{
			SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse (ShapeParametersJson.text);
			for (int betaIndex = 0; betaIndex < node["betas"].Count; betaIndex++) 
				shapeParameters [betaIndex] = node["betas"][betaIndex].AsFloat;
		
			SetShapeBlendValues();
			UpdateBonePositions();
		}
	
	
		void GetGender() {
			SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse (JointRegressorJson.text);
			string genderString = node[GenderJSONKey];
			switch (genderString) {
				case "male":
					gender = Gender.Male;
					break;
				case "female":
					gender = Gender.Female;
					break;
				default:
					throw new ArgumentOutOfRangeException("Do not recognize gender from JSON file");
			}
		}
	}
}