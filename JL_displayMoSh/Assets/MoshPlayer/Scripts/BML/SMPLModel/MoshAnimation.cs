using System;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    
    public class MoshAnimation {

        readonly int           sourceTotalFrameCount;
        readonly Vector3[]     translations;
        readonly Quaternion[,] allPoses;
        readonly float[]       betas;

        int desiredFPS;

        int resampledTotalFrameCount;

        readonly float  duration;
        public   Gender Gender { get; }

        
        bool                     resamplingRequired = false;
        readonly int             sourceFPS;
        BoneModifier           boneModifier;

        int                 currentFrame = 0;
        SkinnedMeshRenderer meshRenderer;
        
        
        public bool Finished => currentFrame >= resampledTotalFrameCount;
    

        public MoshAnimation(Gender    gender,       int           sourceTotalFrameCount, int          sourceFPS, float[] betas,
                             Vector3[] translations, Quaternion[,] allPoses,              SMPLSettings settings) {
            Gender = gender;
            this.sourceTotalFrameCount = sourceTotalFrameCount;
            this.sourceFPS = sourceFPS;
            desiredFPS = sourceFPS;
            resampledTotalFrameCount = sourceTotalFrameCount;
            duration = this.sourceTotalFrameCount / (float) this.sourceFPS;
            this.betas = betas;
            this.translations = translations;
            this.allPoses = allPoses;
            currentFrame = 0;

        }

        public void AttachAnimationToMoshCharacter(SkinnedMeshRenderer meshRendererToAttach, SMPLSettings settings) {
            meshRenderer = meshRendererToAttach;
            boneModifier = new BoneModifier(meshRenderer, Gender, betas, settings);
            
            InitializeBodyShapeBlendshapes();
        }


        public void AdjustFrameRate(int desiredFrameRate) {
            if (desiredFrameRate != 0) {
                SetDesiredFPS(desiredFrameRate);
            }
        }


        /// <summary>
        /// Gets or sets the fps, upsampling or downsampling if the fps is 
        /// is different from the source fps. 
        /// </summary>
        /// <value>The new setDesiredFPS to sample to.</value>
        void SetDesiredFPS(int value) {
        
            desiredFPS = value;
            // duration stays constant, but upsampling/downsampling will happen.
            // Time of start and end keys remains constant, but keys in between are shifted
            // and more may be added or removed.
        
            resamplingRequired = desiredFPS != sourceFPS;
        
            // have to update length here. 
            // I think this is the right way to get length.
            // actually, since the time of the last thisFrame should remain static, 
            // if the time between frames is a constant, then the time of the last thisFrame cannot
            // be completely static. 
            // I think I should still floor the value. 
            resampledTotalFrameCount = Mathf.FloorToInt(desiredFPS * duration);
        
        }


        Vector3 GetTranslationAtFrame(int thisFrame) {
            if (!resamplingRequired) return translations[thisFrame];
        
            ResampledFrame resampledFrame = new ResampledFrame(thisFrame, sourceFPS, resampledTotalFrameCount, duration);
            Vector3 translationAtFrameBeforeThis = translations[resampledFrame.FrameBeforeThis];
            
            bool isLastFrame = resampledFrame.FrameAfterThis >= sourceTotalFrameCount;
            if (isLastFrame) return translationAtFrameBeforeThis;
            
            Vector3 translationAtFrameAfterThis = translations[resampledFrame.FrameAfterThis];
            
            Vector3 resampledTranslation = Vector3.Lerp(translationAtFrameBeforeThis, 
                                                        translationAtFrameAfterThis, 
                                                        resampledFrame.PercentageElapsedSinceLastFrame);
            return resampledTranslation;
        }

      

        /// <summary>
        /// Populate an array with rotations of each joint at thisFrame. 
        /// </summary>
        /// <param name="thisFrame">Frame at which to get rotations</param>
        Quaternion[] GetPosesAtFrame(int thisFrame) 
        {
            Quaternion[] posesThisFrame = new Quaternion[SMPLConstants.JointCount];

            if (!resamplingRequired) {
                for (int jointIndex = 0; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                    posesThisFrame[jointIndex] = allPoses[thisFrame, jointIndex];
                }
            }
            else {
                GetResampledPosesAtFrame(thisFrame, posesThisFrame);
            }
            return posesThisFrame;
        }

        void GetResampledPosesAtFrame(int thisFrame, Quaternion[] posesThisFrame) {
            for (int jointIndex = 0; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                ResampledFrame resampledFrame = new ResampledFrame(thisFrame, sourceFPS, resampledTotalFrameCount, duration);
                if (resampledFrame.FrameAfterThis >= sourceTotalFrameCount) {
                    posesThisFrame[jointIndex] = allPoses[resampledFrame.FrameBeforeThis, jointIndex];
                }
                else {
                    Quaternion rotationAtFrameBeforeThis = allPoses[resampledFrame.FrameBeforeThis, jointIndex];
                    Quaternion rotationAtFrameAfterThis = allPoses[resampledFrame.FrameAfterThis, jointIndex];
                    posesThisFrame[jointIndex] = Quaternion.Slerp(rotationAtFrameBeforeThis, rotationAtFrameAfterThis,
                                                                  resampledFrame.PercentageElapsedSinceLastFrame);
                }
            }
        }

        /// <summary>
        /// The SMPL models have a scaling factor for some reason,
        /// and also need to be multiplied by another factor to be in the same scale as Unity
        /// </summary>
        /// <param name="rawWeight"></param>
        /// <returns></returns>
        static float ScaleBlendshapeWeightFromBlenderToUnity(float rawWeight) {
            float scaledWeight = rawWeight * SMPLConstants.UnityBlendShapeScaleFactor * SMPLConstants.SMPLBlendshapeScalingFactor;
            return scaledWeight;
        }


        /// <summary>
        /// Updates all pose-dependent blendshapes this frame.
        /// 
        /// Pelvis has no blend shapes. So need to skip it when iterating through joints.
        /// But then to index blendshapes need to subtract that 1 back to get blendshape index.
        /// </summary>
        /// <param name="poses"></param>
        void UpdatePoseDependentBlendShapes(Quaternion[] poses) {
            
            for (int jointIndex = SMPLConstants.FirstJointIndexAfterPelvis; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                Quaternion jointPose = poses[jointIndex];
                float[] rotationMatrix3X3 = jointPose.To3X3Matrix();
            
                int jointIndexNoPelvis = jointIndex - 1; // no blendshapes for pelvis.
                for (int rotationMatrixElement = 0; rotationMatrixElement < SMPLConstants.RotationMatrixElementCount; rotationMatrixElement++) {
                    float theta = rotationMatrix3X3[rotationMatrixElement];
                    float scaledTheta = ScaleBlendshapeWeightFromBlenderToUnity(theta); 
                    int blendShapeIndex = SMPLConstants.ShapeBetaCount + jointIndexNoPelvis * SMPLConstants.RotationMatrixElementCount + rotationMatrixElement;
                    meshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledTheta);
                }
            }
        }


        public void PlayCurrentFrame() {
            Vector3 translationThisFrame = GetTranslationAtFrame(currentFrame);
            Quaternion[] posesThisFrame = GetPosesAtFrame(currentFrame);
            
            boneModifier.UpdateBoneRotations(posesThisFrame, translationThisFrame);
            UpdatePoseDependentBlendShapes(posesThisFrame);
            currentFrame++;
        }
        
        void InitializeBodyShapeBlendshapes() {
            for (int betaIndex = 0; betaIndex < SMPLConstants.ShapeBetaCount; betaIndex++) {
                float scaledBeta = ScaleBlendshapeWeightFromBlenderToUnity(betas[betaIndex]);
                meshRenderer.SetBlendShapeWeight(betaIndex, scaledBeta);
            }
        }

    }
}
