using System;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    
    public class MoshAnimation {

        readonly int           sourceTotalFrameCount;
        readonly Vector3[]     translations;
        readonly Quaternion[,] allPoses;
        readonly float[]       betas;
        const int FirstJointAfterPelvis = 1;

        int desiredFPS;

        int resampledTotalFrameCount;

        readonly float  duration;
        public   Gender Gender { get; }

        readonly JointCalculator jointCalculator;
        bool                     resamplingRequired = false;
        readonly int             sourceFPS;
        BoneModifier           boneModifier;

        int                 currentFrame = 0;
        SkinnedMeshRenderer meshRenderer;
        
        
        public bool Finished => currentFrame >= resampledTotalFrameCount;
    

        public MoshAnimation(Gender    gender,       int           sourceTotalFrameCount, int          sourceFPS, float[] betas,
                             Vector3[] translations, Quaternion[,] allPoses,              SMPLSettings settings) {
            Gender = gender;
            switch (gender) {
                case Gender.Male:
                    jointCalculator = settings.MaleJointCalculator;
                    break;
                case Gender.Female:
                    jointCalculator = settings.FemaleJointCalculator;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }

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
            boneModifier = new BoneModifier(meshRenderer, settings);
            
            Reset();
            SetUpShapeBlendshapes();
            CalculateJoints();
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
        /// Get the values for shape parameters in Unity, that define the 
        /// shape of the subject. 
        /// </summary>
        float[] GetScaledBetas () {
            float[] betaValues = new float[SMPLConstants.ShapeBetaCount];

            for (int i = 0; i < SMPLConstants.ShapeBetaCount; i++) {
                float scaledBeta = ScaleBlendshapeWeightFromBlenderToUnity(betas[i]);
                betaValues[i] = scaledBeta;
            }
            return betaValues;
        }

        static float ScaleBlendshapeWeightFromBlenderToUnity(float rawWeight) {
            float scaledWeight = rawWeight * 100f / SMPLConstants.BetaScalingFactor;
            return scaledWeight;
        }


        void SetPoseDependentBlendShapesForCurrentFrame(Quaternion[] poses) {
            // start at 1 to skip pelvis. 
            // pelvis has a rotation, but doesn't seem to have associated blend shapes.
            
            for (int jointPoseIndexAfterPelvis = FirstJointAfterPelvis; jointPoseIndexAfterPelvis < SMPLConstants.JointCount; jointPoseIndexAfterPelvis++) {
                // i is equivalent to index for the other version. 
                Quaternion currentPose = poses[jointPoseIndexAfterPelvis];
                float[] rotationMatrix3X3 = currentPose.To3X3Matrix();
            
                int jointPoseIndexAfterPelvisExcluded = jointPoseIndexAfterPelvis - 1;
                for (int rotationMatrixElement = 0; rotationMatrixElement < SMPLConstants.RotationMatrixElementCount; rotationMatrixElement++) {
                    float theta = rotationMatrix3X3[rotationMatrixElement];
                    float scaledTheta = ScaleBlendshapeWeightFromBlenderToUnity(theta); 
                    int blendShapeIndex = SMPLConstants.ShapeBetaCount + jointPoseIndexAfterPelvisExcluded * SMPLConstants.RotationMatrixElementCount + rotationMatrixElement;
                    meshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledTheta);
                }
            }
        }


        public void PlayCurrentFrame() {
            Vector3 translationAtFrame = GetTranslationAtFrame(currentFrame);
            Quaternion[] posesThisFrame = GetPosesAtFrame(currentFrame);
            boneModifier.UpdateBoneAngles(posesThisFrame, translationAtFrame);
            SetPoseDependentBlendShapesForCurrentFrame(posesThisFrame);
            currentFrame++;
        }
        
        void Reset() {
            boneModifier.ResetRotations();
            ResetBlendShapes();
            Vector3[] joints = jointCalculator.CalculateJointsAtZeroedBetas();
            boneModifier.UpdateBonePositions(joints);
        }

        
        void SetUpShapeBlendshapes() {
            float[] shapeBetas = GetScaledBetas();
            for (int betaIndex = 0; betaIndex < SMPLConstants.ShapeBetaCount; betaIndex++) {
                meshRenderer.SetBlendShapeWeight(betaIndex, shapeBetas[betaIndex]);
            }
        }

        void ResetBlendShapes() {
            for (int blendShapeIndex = 0; blendShapeIndex < meshRenderer.sharedMesh.blendShapeCount; blendShapeIndex++) {
                meshRenderer.SetBlendShapeWeight(blendShapeIndex, 0f);
            }
        }

        
        /// <summary>
        /// Gets the new joint positions from the animation.
        /// Passes them to the boneModifier. 
        /// </summary>
        void CalculateJoints()
        {
            Vector3[] joints = jointCalculator.CalculateJointPositions(betas);
            boneModifier.UpdateBonePositions(joints);
        }
        
    }
}
