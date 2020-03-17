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
        Transform[]         bones;
        ModelDefinition     model;
        MoshCharacter       moshCharacter;
        Quaternion[] poses;
        float feetOffset = 0;
        bool firstFrame = false;
        Vector3 translation;
        Vector3 firstTranslation;

        void OnEnable() {
            moshCharacter = GetComponentInParent<MoshCharacter>();
            if (moshCharacter == null) throw new NullReferenceException("Can't find MoshCharacter component");
            
            
            model = moshCharacter.Model;

            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null)
                throw new NullReferenceException("Not attached to object with a skinnedMeshRenderer");

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
            
            UpdateTranslation();
            
            if (firstFrame) {
                firstTranslation = translation;
                CalculateFeetOffset();
                firstFrame = false;
                UpdateTranslation();
            }
            
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

        public void SetTranslation(Vector3 trans) {
            translation = trans;
        }
        
        void UpdateTranslation() {
            
            if(moshCharacter.RenderOptions.AllowPoseManipulation) return;
            
            Vector3 finalTrans = Vector3.zero;
            
            //height needs to be dealt with separately because of ground-snapping
            finalTrans.y = moshCharacter.RenderOptions.UpdateTranslationLiveY ? translation.y : firstTranslation.y;
            if (moshCharacter.RenderOptions.SnapToGroundFirstFrame) finalTrans.y += feetOffset;
            
            //Horizontal plane simple enough
            finalTrans.x = moshCharacter.RenderOptions.UpdateTranslationLiveXZ ? translation.x : firstTranslation.x;
            finalTrans.z = moshCharacter.RenderOptions.UpdateTranslationLiveXZ ? translation.z : firstTranslation.z;
            
            moshCharacter.gameObject.transform.localPosition = finalTrans;
            
        }

        void CalculateFeetOffset() {
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            Vector3[] vertices = bakedMesh.vertices;

            float miny = Mathf.Infinity;
            foreach (Vector3 vertex in vertices) {
                miny = Mathf.Min(vertex.y, miny);
            }
            
            Transform pelvis = skinnedMeshRenderer.bones[model.PelvisIndex];
            Vector3 worldVector = pelvis.parent.TransformPoint(new Vector3(0, miny, 0));

            feetOffset = -worldVector.y;//+ moshCharacter.Body.pelvisNewLocation.y;

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

        public void NotifyFirstFrame() {
            firstFrame = true;
        }

        public void ForceUpdate() {
            Update();
        }
    }
    
}