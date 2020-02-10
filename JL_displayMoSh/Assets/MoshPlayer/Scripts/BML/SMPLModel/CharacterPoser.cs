using System;
using System.Collections.Generic;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    /// <summary>
    /// Poses the body and adjust pose-dependent blendshapes accordingly.
    /// </summary>
    public class CharacterPoser : MonoBehaviour {

        SkinnedMeshRenderer skinnedMeshRenderer;
        Vector3[]           tPoseVertexes;
        Transform[]         bones;
        ModelDefinition     model;
        
        [SerializeField]
        bool UpdatePosesLive = default;

        [SerializeField]
        bool UpdateBlendshapesLive= default;
        
        
        Quaternion[]        currentTempPoses;
        MoshCharacter       moshCharacter;
        Quaternion[] poses;

        void OnEnable() {
            moshCharacter = GetComponentInParent<MoshCharacter>();
            if (moshCharacter == null) throw new NullReferenceException("Can't find MoshCharacter component");
            
            
            model = moshCharacter.Model;

            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null)
                throw new NullReferenceException("Not attached to object with a skinnedMeshrenderer");

            bones = skinnedMeshRenderer.bones;
            
            poses = new Quaternion[model.JointCount];
        }

        void OnDisable() {
            ResetToTPose();
        }

        void LateUpdate() {
            if (UpdateBlendshapesLive) {
                AddPoseDependentBlendShapes();
            }
            else {
                ResetPoseDependentBlendShapesToZero();
            }
            

            if (UpdatePosesLive) {
                UpdatePoses();
            }
            else {
                ResetBones();
            }
        }

        public void SetPoses(Quaternion[] poses) {
            this.poses = poses;
        }

        
        /// <summary>
        /// Updates the bones based on new poses and translation of pelvis
        /// </summary>
        /// <param name="poses"></param>
        void UpdatePoses() {
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
            moshCharacter.gameObject.transform.localPosition = trans;
        }

        [ContextMenu("ResetToTPose")]
        public void ResetToTPose() {
            ResetBones();
            ResetPoseDependentBlendShapesToZero();
        }

        void ResetBones() {
            foreach (var bone in bones) {
                bone.rotation = Quaternion.identity;
            }
        }


        /// <summary>
        /// Updates all pose-dependent blendshapes this frame.
        /// 
        /// Pelvis has no blend shapes. So need to skip it when iterating through joints.
        /// But then to index blendshapes need to subtract that 1 back to get blendshape index.
        /// </summary>
        void AddPoseDependentBlendShapes() {

            int startingJoint =  model.FirstPoseIsPelvisTranslation ? 1 : 0;

            for (int jointIndex = startingJoint; jointIndex < model.JointCount; jointIndex++) {
                Quaternion jointPose = poses[jointIndex];
                float[] rotationMatrix3X3 = jointPose.To3X3MatrixMinusIdent();
                
                for (int rotMatrixElement = 0; rotMatrixElement < SMPLConstants.RotationMatrixElementCount; rotMatrixElement++) {
                    
                    float rawPoseDependentBlendshapeWeight = rotationMatrix3X3[rotMatrixElement];
                    float scaledWeightBeta = ScalePoseBlendshapesFromBlenderToUnity(rawPoseDependentBlendshapeWeight); 
                    
                    int jointIndexNoPelvis = model.FirstPoseIsPelvisTranslation ? jointIndex - 1 : jointIndex; // no blendshapes for pelvis.
                    int blendShapeIndex = model.BodyShapeBetaCount + jointIndexNoPelvis * SMPLConstants.RotationMatrixElementCount + rotMatrixElement;
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledWeightBeta);
                }
            }
        }
        
        [ContextMenu("ResetPoseBlendshapes")]
        void ResetPoseDependentBlendShapesToZero() {
            int startingJoint =  model.FirstPoseIsPelvisTranslation ? 1 : 0;
            for (int jointIndex = startingJoint; jointIndex < model.JointCount; jointIndex++) {
                for (int rotMatrixElement = 0; rotMatrixElement < SMPLConstants.RotationMatrixElementCount; rotMatrixElement++) {
                    float scaledWeightBeta = 0;
                    int jointIndexNoPelvis = model.FirstPoseIsPelvisTranslation ? jointIndex - 1 : jointIndex; // no blendshapes for pelvis.
                    int blendShapeIndex = model.BodyShapeBetaCount + jointIndexNoPelvis * SMPLConstants.RotationMatrixElementCount + rotMatrixElement;
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledWeightBeta);
                }
            }
        }

        float ScalePoseBlendshapesFromBlenderToUnity(float rawWeight) {
            //TODO if (!model.AddPoseBlendshapes) return 0;
            float scaledWeight = rawWeight * model.PoseBlendshapeScalingFactor * model.UnityBlendShapeScaleFactor;
            return scaledWeight;

        }
    }
    
}