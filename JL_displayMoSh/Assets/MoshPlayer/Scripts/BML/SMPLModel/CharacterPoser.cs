using System;
using System.Collections.Generic;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    /// <summary>
    /// Slightly modified version of SMPLModifyBones from MPI.
    /// Primarily just adds a reset method. 
    /// 
    /// </summary>
    
    [ExecuteInEditMode]
    public class CharacterPoser : MonoBehaviour {

        SkinnedMeshRenderer skinnedMeshRenderer;
        Vector3[] tPoseVertexes;
        Transform[] bones;
        ModelDefinition model;
        IndividualizedBody body;
        public bool  UpdatePosesLive;
        Quaternion[] currentTempPoses;

        
        void OnEnable() {
            MoshCharacter moshCharacter = GetComponentInParent<MoshCharacter>();
            if (moshCharacter == null) throw new NullReferenceException("Can't find MoshCharacter component");

            model = moshCharacter.Model;
            
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null) throw new NullReferenceException("Not attached to object with a skinnedMeshrenderer");
            
            bones = skinnedMeshRenderer.bones;

            body = GetComponent<IndividualizedBody>();
            if (body== null) throw new NullReferenceException("Can't find individualized body");
        }

        void OnDisable() {
            ResetToTPose();
        }

        void Update() {
            if (UpdatePosesLive) {
                Quaternion[] currentBonePoses = GatherPosesFromBones();
                AddPoseDependentBlendShapes(currentBonePoses);
            }
            else {
                ResetPoseDependentBlendShapesToZero();
            }
        }

        /// <summary>
        /// Updates the bones based on new poses and translation of pelvis
        /// </summary>
        /// <param name="poses"></param>
        /// <param name="trans"></param>
        public void UpdateBoneRotations(Quaternion[] poses)  {
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                if (boneName == Bones.Pelvis) continue;
                try {
                    int poseIndex = Bones.NameToJointIndex[boneName];
                    bones[boneIndex].localRotation = poses[poseIndex];
                }
                catch (KeyNotFoundException) {
                    throw new KeyNotFoundException($"Bone Not in dictionary: boneIndex: {boneIndex}, name: {boneName}");
                }
            }
        }

        public void UpdateTranslation(Vector3 trans) {
            bones[model.PelvisIndex].localPosition = trans;
        }


        public Quaternion[] GatherPosesFromBones() {
            //Debug.Log("Gathering Poses");
            Quaternion[] poses = new Quaternion[model.JointCount];
            foreach (Transform bone in skinnedMeshRenderer.bones) {
                int poseIndex = Bones.NameToJointIndex[bone.name];
                poses[poseIndex] = bone.localRotation;
                if (poses[poseIndex] != Quaternion.identity) {
                    //Debug.Log($"poses {poseIndex}: {poses[poseIndex]}");
                }
            }
            return poses;
        }

        /// <summary>
        /// Updates the bones based on new poses and translation of pelvis
        /// </summary>
        /// <param name="poses"></param>
        public void PoseSkeleton(Quaternion[] poses)  {
            Debug.Log("reposing");
            for (int boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; boneIndex++) {
                string boneName = skinnedMeshRenderer.bones[boneIndex].name;
                if (boneName == Bones.Pelvis) continue;
                try {
                    int poseIndex = Bones.NameToJointIndex[boneName];
                    skinnedMeshRenderer.bones[boneIndex].localRotation = poses[poseIndex];
                }
                catch (KeyNotFoundException) {
                    throw new KeyNotFoundException($"Bone Not in dictionary: boneIndex: {boneIndex}, name: {boneName}");
                }
            }
        }

        [ContextMenu("ResetToTPose")]
        public void ResetToTPose() {
            foreach (var bone in bones) {
                bone.rotation = Quaternion.identity;
            }
            ResetPoseDependentBlendShapesToZero();
            body.UpdateBody();

        }


        /// <summary>
        /// Updates all pose-dependent blendshapes this frame.
        /// 
        /// Pelvis has no blend shapes. So need to skip it when iterating through joints.
        /// But then to index blendshapes need to subtract that 1 back to get blendshape index.
        /// </summary>
        /// <param name="poses"></param>
        void AddPoseDependentBlendShapes(Quaternion[] poses) {

            int startingJoint =  model.SkipFirstPose ? 1 : 0;

            for (int jointIndex = startingJoint; jointIndex < model.JointCount; jointIndex++) {
                Quaternion jointPose = poses[jointIndex];
                if (jointPose != Quaternion.identity) Debug.Log("nonZero pose");
                float[] rotationMatrix3X3 = jointPose.To3X3MatrixMinusIdent();
                
                for (int rotMatrixElement = 0; rotMatrixElement < SMPLConstants.RotationMatrixElementCount; rotMatrixElement++) {
                    
                    float rawPoseDependentBlendshapeWeight = rotationMatrix3X3[rotMatrixElement];
                    //Debug.Log($"joint {jointIndex}, ele {rotMatrixElement} raw BS: {rawPoseDependentBlendshapeWeight}");
                    float scaledWeightBeta = ScalePoseBlendshapesFromBlenderToUnity(rawPoseDependentBlendshapeWeight); 
                    
                    int jointIndexNoPelvis = model.SkipFirstPose ? jointIndex - 1 : jointIndex; // no blendshapes for pelvis.
                    //Debug.Log(jointIndexNoPelvis);
                    int blendShapeIndex = model.BodyShapeBetaCount + jointIndexNoPelvis * SMPLConstants.RotationMatrixElementCount + rotMatrixElement;
                    //Debug.Log(blendShapeIndex);
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledWeightBeta);
                    
                    //if(Math.Abs(scaledWeightBeta) > 0.001f) Debug.Log($"setting{blendShapeIndex} to {scaledWeightBeta}");
                }
            }
        }
        
        [ContextMenu("ResetPoseBlendshapes")]
        void ResetPoseDependentBlendShapesToZero() {
            int startingJoint =  model.SkipFirstPose ? 1 : 0;
            for (int jointIndex = startingJoint; jointIndex < model.JointCount; jointIndex++) {
                for (int rotMatrixElement = 0; rotMatrixElement < SMPLConstants.RotationMatrixElementCount; rotMatrixElement++) {
                    float scaledWeightBeta = 0;
                    int jointIndexNoPelvis = model.SkipFirstPose ? jointIndex - 1 : jointIndex; // no blendshapes for pelvis.
                    int blendShapeIndex = model.BodyShapeBetaCount + jointIndexNoPelvis * SMPLConstants.RotationMatrixElementCount + rotMatrixElement;
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledWeightBeta);
                }
            }
        }

        float ScalePoseBlendshapesFromBlenderToUnity(float rawWeight) {
            if (!model.AddPoseBlendshapes) return 0;
            float scaledWeight = rawWeight * model.PoseBlendshapeScalingFactor * model.UnityBlendShapeScaleFactor;
            return scaledWeight;

        }
    }
    
}