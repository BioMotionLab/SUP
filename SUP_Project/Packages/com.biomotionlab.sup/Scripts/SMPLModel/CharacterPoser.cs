using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace SMPLModel {
    /// <summary>
    /// Poses the body and adjust pose-dependent blendshapes accordingly.
    /// </summary>
    public class CharacterPoser : MonoBehaviour {
        
        MoshCharacter       moshCharacter;
        SkinnedMeshRenderer skinnedMeshRenderer;
        Transform[]         bones;
        ModelDefinition     model;

        [SerializeField] Quaternion[] poses;
        
        void Awake() {
            moshCharacter = GetComponent<MoshCharacter>();
            if (moshCharacter == null) throw new NullReferenceException("Can't find MoshCharacter component");

            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null) throw new NullReferenceException("Can't find SkinnedMeshRenderer component");
            model = moshCharacter.Model;
            bones = skinnedMeshRenderer.bones;
            
            poses = new Quaternion[model.JointCount];
        }
        
        void OnDestroy() {
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
            else if (moshCharacter.RenderOptions.UpdatePosesLive) UpdatePoses();
            else ResetPoses();
            

            if (moshCharacter.RenderOptions.UpdatePoseBlendshapesLive) AddPoseDependentBlendShapes(poses);
            else ResetPoseDependentBlendShapesToZero();

        }

        Quaternion[] GatherPosesFromBones() {
            Quaternion[] currentPoses = new Quaternion[model.JointCount];
            foreach (Transform bone in skinnedMeshRenderer.bones) {
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
                
                try {
                    
                    //to deal with pelvis's rotation, it's rotated by -90 degrees on x axis because maya
                    //uses z=up rather than unity's y=up. So to deal with that we have to add this correction.
                    //But since the order matters for multiple rotations, this complicates things,
                    //meaning we have to reset the rotation to zero first, then apply them additively in a particular order.
                    bones[boneIndex].transform.localEulerAngles = Vector3.zero;
                    
                    if (boneName == Bones.Pelvis) {
                        bones[boneIndex].transform.Rotate(-90, 0, 0, Space.Self);
                    }
                    
                    int poseIndex = Bones.NameToJointIndex[boneName];
                    bones[boneIndex].localRotation = bones[boneIndex].localRotation * poses[poseIndex];
                    
                }
                catch (KeyNotFoundException) {
                    throw new KeyNotFoundException($"Bone Not in dictionary: boneIndex: {boneIndex}, animationName: {boneName}");
                }
                
            }
        }

        [ContextMenu("ResetToTPose")]
        public void ResetToTPose() {
            ResetPoses();
            ResetPoseDependentBlendShapesToZero();
        }

        void ResetPoses() {
            foreach (Transform bone in bones) {
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

        public void ForceUpdate() {
            Update();
        }
        
    }
}