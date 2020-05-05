using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.SMPLModel {
    /// <summary>
    /// Poses the body and adjust pose-dependent blendshapes accordingly.
    /// </summary>
    public class CharacterPoser : MonoBehaviour {

        SkinnedMeshRenderer skinnedMeshRenderer;
        Transform[]         bones;
        ModelDefinition     model;
        MoshCharacter       moshCharacter;
        
        bool firstFrame = false;
        bool bodyChanged = false;
        
        [SerializeField] Vector3 translation;
        [SerializeField] Vector3 firstFrameTranslation;
        [SerializeField] Quaternion[] poses;
        [SerializeField] Grounder grounder;

        void Awake() {
            moshCharacter = GetComponentInParent<MoshCharacter>();
            if (moshCharacter == null) throw new NullReferenceException("Can't find MoshCharacter component");

            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null) throw new NullReferenceException("Can't find SkinnedMeshRenderer component");
            model = moshCharacter.Model;
            bones = skinnedMeshRenderer.bones;
            
            grounder = new Grounder(moshCharacter, skinnedMeshRenderer);
            poses = new Quaternion[model.JointCount];

            moshCharacter.Events.OnBodyChanged += BodyChanged;
            PlaybackEventSystem.OnChangeSnapToGround += GroundingChanged;
        }
        
        void OnDestroy() {
            ResetToTPose();
            moshCharacter.Events.OnBodyChanged -= BodyChanged;
            PlaybackEventSystem.OnChangeSnapToGround -= GroundingChanged;
            grounder.Destory();
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
            
            UpdateTranslation();
            
            if (firstFrame) ConfigureFirstFrame();
            

            if (!moshCharacter.RenderOptions.UpdatePosesLive) UpdateFootOffset();
            else if (!firstFrame && bodyChanged) UpdateFootOffset();
            
        }

        void UpdateFootOffset() {
            bodyChanged = false;
            grounder.UpdateGround();
            UpdateTranslation();
        }

        void ConfigureFirstFrame() {
            firstFrameTranslation = translation;
            grounder.InitGround();
            bodyChanged = false;
            firstFrame = false;
            UpdateTranslation();
        }

        void GroundingChanged(GroundSnapType unused) {
            UpdateTranslation();
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
            //Debug.Log($"setting trans: {translation.ToString("F4")}");
        }

        void UpdateTranslation() {
            
            if(moshCharacter.RenderOptions.AllowPoseManipulation) return;
            
            Vector3 finalTrans = Vector3.zero;

            finalTrans = UpdateVerticalTranslation(finalTrans);
            finalTrans = UpdateHorizontalTranslation(finalTrans);

            moshCharacter.gameObject.transform.localPosition = finalTrans;
            
        }

        Vector3 UpdateVerticalTranslation(Vector3 finalTrans) {
            //height needs to be dealt with separately because of ground-snapping
            if (moshCharacter.RenderOptions.UpdateTranslationLiveY && moshCharacter.RenderOptions.UpdatePosesLive)
                finalTrans.y = translation.y;
            else
                finalTrans.y = firstFrameTranslation.y;

            finalTrans = grounder.ApplyGround(finalTrans, firstFrame);
            return finalTrans;
        }

        Vector3 UpdateHorizontalTranslation(Vector3 finalTrans) {
            if (moshCharacter.RenderOptions.UpdatePosesLive) {
                //Horizontal plane simple enough
                finalTrans.x = moshCharacter.RenderOptions.UpdateTranslationLiveXZ ? translation.x : firstFrameTranslation.x;
                finalTrans.z = moshCharacter.RenderOptions.UpdateTranslationLiveXZ ? translation.z : firstFrameTranslation.z;
            }

            return finalTrans;
        }
        

        [UnityEngine.ContextMenu("ResetToTPose")]
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

        [UnityEngine.ContextMenu("ResetPoseBlendshapes")]
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

        void BodyChanged() {
            bodyChanged = true;
        }
    }
}