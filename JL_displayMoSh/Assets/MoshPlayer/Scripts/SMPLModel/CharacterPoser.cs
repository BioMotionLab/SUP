using System;
using System.Collections.Generic;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.SMPLModel {
    /// <summary>
    /// Poses the body and adjust pose-dependent blendshapes accordingly.
    /// </summary>
    public class CharacterPoser : MonoBehaviour {

        SkinnedMeshRenderer skinnedMeshRenderer;
        Vector3[]           tPoseVertexes;
        Transform[]         bones;
        ModelDefinition     model;
        
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

        void Update() {

            
            if (moshCharacter.RenderOptions.UpdateBodyShapeLive) {
                ResetToTPose();
                moshCharacter.Body.UpdateBody();
            }
            

            if (moshCharacter.RenderOptions.AllowPoseManipulation) {
                poses = GatherPosesFromBones();
                UpdatePoses();
            }
            else if (moshCharacter.RenderOptions.UpdatePosesLive) {
                UpdatePoses();
            }
            else {
                ResetPoses();
            }

            if (moshCharacter.RenderOptions.UpdatePoseBlendshapesLive) {
                AddPoseDependentBlendShapes(poses);
            }
            else {
                ResetPoseDependentBlendShapesToZero();
            }
        }


        Quaternion[] GatherPosesFromBones() {
            Quaternion[] currentPoses = new Quaternion[model.JointCount];
            for (int boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; boneIndex++) {
                Transform bone = skinnedMeshRenderer.bones[boneIndex];
                int poseIndex = Bones.NameToJointIndex[bone.name];
                currentPoses[poseIndex] = bone.localRotation;
            }

            return currentPoses;
        }

        public void SetPoses(Quaternion[] newPoses) {
            poses = newPoses;
        }

        
        /// <summary>
        /// Updates the bones based on new newPoses and translation of pelvis
        /// </summary>
        /// <param animationName="newPoses"></param>
        void UpdatePoses() {
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                if (boneName == Bones.Pelvis) continue;
                try {
                    int poseIndex = Bones.NameToJointIndex[boneName];
                    bones[boneIndex].localRotation = poses[poseIndex];
                }
                catch (KeyNotFoundException) {
                    throw new KeyNotFoundException($"Bone Not in dictionary: boneIndex: {boneIndex}, animationName: {boneName}");
                }
            }
        }

        public void UpdateTranslation(Vector3 trans) {
            if(moshCharacter.RenderOptions.AllowPoseManipulation) return;


            if (!moshCharacter.RenderOptions.UpdateTranslationLiveY &&
                !moshCharacter.RenderOptions.UpdateTranslationLiveXZ) 
                return;
            
            Vector3 finalTrans = Vector3.zero;
            if (moshCharacter.RenderOptions.UpdateTranslationLiveY) {
                finalTrans.y = trans.y;
                
            }
            if (moshCharacter.RenderOptions.UpdateTranslationLiveXZ) {
                finalTrans.x = trans.x;
                finalTrans.z = trans.z;
            }
            moshCharacter.gameObject.transform.localPosition = finalTrans;



        }

        [ContextMenu("ResetToTPose")]
        public void ResetToTPose() {
            ResetPoses();
            ResetPoseDependentBlendShapesToZero();
        }

        void ResetPoses() {
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
        void AddPoseDependentBlendShapes(Quaternion[] posesToMatch) {

            int startingJoint =  model.FirstPoseIsPelvisTranslation ? 1 : 0;

            for (int jointIndex = startingJoint; jointIndex < model.JointCount; jointIndex++) {
                
                Quaternion jointPose = posesToMatch[jointIndex];
                
                //convert to from Unity back to MPI's right-handed coords.
                jointPose = jointPose.ToRightHanded();

                float[] rotationMatrix = jointPose.To3X3Matrix();
                rotationMatrix = MatrixUtilities.SubtractIdentity(rotationMatrix);
                
                //rotationMatrix = MatrixUtilities.RotationMatrix3x3ToRightHanded(rotationMatrix);
                
                for (int rotMatrixElement = 0; rotMatrixElement < SMPLConstants.RotationMatrixElementCount; rotMatrixElement++) {
                    
                    float rawPoseDependentBlendshapeWeight = rotationMatrix[rotMatrixElement];
                    float scaledWeightBeta = ScalePoseBlendshapesFromBlenderToUnity(rawPoseDependentBlendshapeWeight);

                    //if (moshCharacter.Gender == Gender.Female && model.FemaleNegativeBlendshapes) scaledWeightBeta = -scaledWeightBeta;
                    //if (Mathf.Abs(scaledWeightBeta) < BlendShapeThreshold) continue;

                    int jointIndexNoPelvis = model.FirstPoseIsPelvisTranslation ? jointIndex - 1 : jointIndex; // no blendshapes for pelvis.
                    int blendShapeIndex = model.BodyShapeBetaCount + jointIndexNoPelvis * SMPLConstants.RotationMatrixElementCount + rotMatrixElement;
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledWeightBeta);
                }
            }
            
            
        }
        
        [ContextMenu("ResetPoseBlendshapes")]
        public void ResetPoseDependentBlendShapesToZero() {
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
            float scaledWeight = rawWeight * model.PoseBlendshapeScalingFactor * model.UnityBlendShapeScaleFactor;
            return scaledWeight;

        }
    }
    
}