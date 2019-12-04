using System;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    
    public class MoshAnimation {

        readonly int           sourceTotalFrameCount;
        readonly Vector3[]     translations;
        readonly Quaternion[,] allPoses;
        readonly float[]       rawBodyShapeWeightBetas;

        int desiredFPS;
        
        readonly int sourceFPS;
        readonly float sourceDuration;
        bool resamplingRequired = false;
        int resampledTotalFrameCount;
        
        public Gender Gender { get; }
        
        BoneModifier boneModifier;
        SkinnedMeshRenderer meshRenderer;
        
        int                 currentFrame = 0;
        float lastFrameTime = 0;

        public bool Finished => currentFrame >= resampledTotalFrameCount;
    

        public MoshAnimation(Gender gender,       
                             int sourceTotalFrameCount, 
                             int sourceFPS, 
                             float[] rawBodyShapeWeightBetas,
                             Vector3[] translations, 
                             Quaternion[,] allPoses) {
            Gender = gender;
            
            this.sourceFPS = sourceFPS;
            this.sourceTotalFrameCount = sourceTotalFrameCount;
            sourceDuration = this.sourceTotalFrameCount / (float) this.sourceFPS;

            desiredFPS = sourceFPS;
            resampledTotalFrameCount = sourceTotalFrameCount;
            
            
            this.rawBodyShapeWeightBetas = rawBodyShapeWeightBetas;
            this.translations = translations;
            this.allPoses = allPoses;
            currentFrame = 0;

        }

        public void AttachSkin(SkinnedMeshRenderer skinnedMeshRendererToAttach, SMPLSettings settings) {
            meshRenderer = skinnedMeshRendererToAttach;
            boneModifier = new BoneModifier(meshRenderer, Gender, rawBodyShapeWeightBetas, settings);
            InitializeBodyShapeBlendshapes();
        }

        void InitializeBodyShapeBlendshapes() {
            for (int betaIndex = 0; betaIndex < SMPLConstants.BodyShapeBetaCount; betaIndex++) {
                float scaledBeta = ScaleBlendshapeWeightFromBlenderToUnity(rawBodyShapeWeightBetas[betaIndex]);
                meshRenderer.SetBlendShapeWeight(betaIndex, scaledBeta);
            }
        }

        public void PlayCurrentFrame() {
            Debug.Log($"Time since last frame: {Time.time - lastFrameTime}");
            
            lastFrameTime = Time.time;
            
            Vector3 translationThisFrame = GetTranslationAtFrame(currentFrame);
            Quaternion[] posesThisFrame = GetPosesAtFrame(currentFrame);
            
            boneModifier.UpdateBoneRotations(posesThisFrame, translationThisFrame);
            UpdatePoseDependentBlendShapes(posesThisFrame);
            currentFrame++;
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
            // sourceDuration stays constant, but upsampling/downsampling will happen.
            // Time of start and end keys remains constant, but keys in between are shifted
            // and more may be added or removed.
        
            resamplingRequired = desiredFPS != sourceFPS;
        
            // have to update length here. 
            // I think this is the right way to get length.
            // actually, since the time of the last frame should remain static, 
            // if the time between frames is a constant, then the time of the last frame cannot
            // be completely static. 
            // I think I should still floor the value. 
            resampledTotalFrameCount = Mathf.FloorToInt(desiredFPS * sourceDuration);
        
        }


        Vector3 GetTranslationAtFrame(int thisFrame) {
            if (!resamplingRequired) return translations[thisFrame];
        
            ResampledFrame resampledFrame = new ResampledFrame(thisFrame, sourceFPS, resampledTotalFrameCount, sourceDuration);
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
        Quaternion[] GetPosesAtFrame(int thisFrame) {
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
                ResampledFrame resampledFrame = new ResampledFrame(thisFrame, sourceFPS, resampledTotalFrameCount, sourceDuration);
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
                for (int rotMatrixElement = 0; rotMatrixElement < SMPLConstants.RotationMatrixElementCount; rotMatrixElement++) {
                    
                    float rawPoseDependentBlendshapeWeight = rotationMatrix3X3[rotMatrixElement];
                    float scaledWeightBetas = ScaleBlendshapeWeightFromBlenderToUnity(rawPoseDependentBlendshapeWeight); 
                    int blendShapeIndex = SMPLConstants.BodyShapeBetaCount + jointIndexNoPelvis * SMPLConstants.RotationMatrixElementCount + rotMatrixElement;
                    meshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledWeightBetas);
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
    }
}
