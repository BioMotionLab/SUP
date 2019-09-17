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
	This script file defines the SMPLBoneModifier class which updates the joints of the model after a new 
	shape has been defined using the 'shapeParmsJSON' field and SMPLJointCalculator class has computed 
	the new joints. 
*/

using System.Collections.Generic;
using UnityEngine;

namespace SMPL.Scripts.mpi {
	public class SMPLBoneModifier {
	
		readonly SkinnedMeshRenderer targetRenderer;
		public   Transform[]         Bones { get; }
		List<TransformBackup>        bonesBackup;
		public string                BoneNamePrefix { get; }

		readonly Dictionary<string, int> boneNameToJointIndex = new Dictionary<string, int> {
																								{"Pelvis", 0},
																								{"L_Hip", 1},
																								{"R_Hip", 2},
																								{"Spine1", 3},
																								{"L_Knee", 4},
																								{"R_Knee", 5},
																								{"Spine2", 6},
																								{"L_Ankle", 7},
																								{"R_Ankle", 8},
																								{"Spine3", 9},
																								{"L_Foot", 10},
																								{"R_Foot", 11},
																								{"Neck", 12},
																								{"L_Collar", 13},
																								{"R_Collar", 14},
																								{"Head", 15},
																								{"L_Shoulder", 16},
																								{"R_Shoulder", 17},
																								{"L_Elbow", 18},
																								{"R_Elbow", 19},
																								{"L_Wrist", 20},
																								{"R_Wrist", 21},
																								{"L_Hand", 22},
																								{"R_Hand", 23}
																							};

	

		readonly Transform pelvis;
		readonly Mesh      bakedMesh = null;

		public SMPLBoneModifier(SkinnedMeshRenderer targetRenderer)
		{
			if (targetRenderer == null)
				throw new System.ArgumentNullException("ERROR: The script should be added to the 'SkinnedMeshRenderer Object");
		
			this.targetRenderer = targetRenderer;
		
			Bones = targetRenderer.bones;
			CreateBackupOfBones();
		
			BoneNamePrefix = "";
		
			bakedMesh = new Mesh();
		
			// Determine bone name prefix
			foreach (Transform bone in Bones)
			{
				if (bone.name.EndsWith("root"))
				{
					int index = bone.name.IndexOf("root");
					BoneNamePrefix = bone.name.Substring(0, index);
					break;
				}
			}

			Debug.Log("INFO: Bone name prefix: '" + BoneNamePrefix + "'");
		}

		public void updateBonePositions(Vector3[] newPositions, bool feetOnGround = true)
		{
			int pelvisIndex = -1;
			for (int i = 0; i < Bones.Length; i++)
			{
				string boneName = Bones[i].name;

				// Remove f_avg/m_avg prefix
				boneName = boneName.Replace(BoneNamePrefix, "");

				if (boneName == "root") continue;
			
				if (boneName == "Pelvis") pelvisIndex = i;
			
				Transform avatarTransform = targetRenderer.transform.parent;
			
				if (boneNameToJointIndex.TryGetValue(boneName, out int index)) {
					// Incoming new positions from joint calculation are centered at origin in world space
					// Transform to avatar position+orientation for correct world space position
					Bones[i].position = avatarTransform.TransformPoint(newPositions[index]);
				}
				else {
					Debug.LogError("ERROR: No joint index for given bone name: " + boneName);
				}
			}

			SetBindPose(Bones);

			if (!feetOnGround) return;

			Vector3 min = new Vector3();
			Vector3 max = new Vector3();
			GetLocalBounds(ref min, ref max);
			float heightOffset = -min.y;

			Bones[pelvisIndex].Translate(0.0f, heightOffset, 0.0f);
			
		}

		public bool updateBoneAngles(float[][] pose, float[] trans)
		{	
			Quaternion quat;
			int pelvisIndex = -1;

			for (int i=0; i<Bones.Length; i++)
			{
				int index;
				string boneName = Bones[i].name;

				// Remove f_avg/m_avg prefix
				boneName = boneName.Replace(BoneNamePrefix, "");

				if (boneName == "root") {
					continue;
				}

				if (boneName == "Pelvis")
					pelvisIndex = i;
			
				if (boneNameToJointIndex.TryGetValue(boneName, out index))
				{
					quat.x = pose [index][0];
					quat.y = pose [index][1];
					quat.z = pose [index][2];
					quat.w = pose [index][3];

					/*	Quaternions */
					Bones[i].localRotation = quat;
				}
				else
				{
					Debug.LogError("ERROR: No joint index for given bone name: " + boneName);
				}
			}
			
			Bones[pelvisIndex].localPosition = new Vector3(trans[0], trans[1], trans[2]);
			return true;
		}


		void CreateBackupOfBones() {
			bonesBackup = new List<TransformBackup>();
			foreach (Transform bone in Bones) {
				bonesBackup.Add(new TransformBackup(bone));
			}
		}

		void RestoreBones()
		{
			for (int i=0; i < Bones.Length; i++) {
				bonesBackup[i].RestoreValuesTo(Bones[i]);
			}
		}

		void SetBindPose(Transform[] bones)
		{
			Matrix4x4[] bindPoses = targetRenderer.sharedMesh.bindposes;
//		Debug.Log("Bind poses: " + bindPoses.Length);

			Transform avatarRootTransform = targetRenderer.transform.parent;

			for (int i=0; i<bones.Length; i++)
			{
				// The bind pose is bone's inverse transformation matrix.
				// Make this matrix relative to the avatar root so that we can move the root game object around freely.            
				bindPoses[i] = bones[i].worldToLocalMatrix * avatarRootTransform.localToWorldMatrix;
			}

			targetRenderer.bones = bones;
			Mesh sharedMesh = targetRenderer.sharedMesh;
			sharedMesh.bindposes = bindPoses;
			targetRenderer.sharedMesh = sharedMesh;

		}

		void GetLocalBounds(ref Vector3 min, ref Vector3 max)
		{
			targetRenderer.BakeMesh(bakedMesh);
			Vector3[] vertices = bakedMesh.vertices;
			int numVertices = vertices.Length;
		
			for (int i=0; i < numVertices; i++)
			{
				Vector3 v = vertices[i];
				min.x = Mathf.Min(v.x, min.x);
				min.y = Mathf.Min(v.x, min.y);
				min.z = Mathf.Min(v.y, min.z);
				max.x = Mathf.Max(v.y, max.x);
				max.y = Mathf.Max(v.z, max.y);
				max.z = Mathf.Max(v.z, max.z);
			}
		}

		// Note: Cannot use OnDestroy() because in OnDestroy the bone Transform objects are already destroyed
		//       See also https://docs.unity3d.com/Manual/ExecutionOrder.html
		public void RestoreOriginalBindPose()
		{
			Debug.Log("Restoring original bind pose");
			
			if ((Bones != null) && (bonesBackup != null))
			{
				RestoreBones();
				SetBindPose(Bones);
			}
		}

		public int GetJointIndexFromBoneName(string boneName) {
			int rawIndex = boneNameToJointIndex[boneName];
			// Can't use the 'index' value as-is from the getB2J_indices dict; 
			// The pose blendshapes have no values corresponding to Pelvis joint. 
			// Pose blendshapes start from hip-joint instead of Pelvis.
			// So we have to begin pose_blend indices from 'index-1'
			int index = rawIndex - 1; 
			return index;
		}
	
	
	}
}