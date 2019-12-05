using System;
using MoshPlayer.Scripts.Utilities;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    
    public class MoshAnimation {

        
        readonly Vector3[]     translations;
        readonly Quaternion[,] allPoses;
        readonly float[]       rawBodyShapeWeightBetas;

        
        readonly int sourceFPS;
        readonly int sourceTotalFrameCount;
        readonly float sourceDuration;
     
        
        public Gender Gender { get; }
        public bool Finished = false;

        BoneModifier boneModifier;
        SkinnedMeshRenderer meshRenderer;
        
        float startTime;
        bool started = false;
        float requiredDuration;
        bool playBackwards;


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
            
            
            this.rawBodyShapeWeightBetas = rawBodyShapeWeightBetas;
            this.translations = translations;
            this.allPoses = allPoses;

        }

        public void AttachSkin(SkinnedMeshRenderer skinnedMeshRendererToAttach, SMPLSettings settings) {
            meshRenderer = skinnedMeshRendererToAttach;
            boneModifier = new BoneModifier(meshRenderer, Gender, rawBodyShapeWeightBetas, settings);

            requiredDuration = sourceDuration / settings.DisplaySpeed;
            
            InitializeBodyShapeBlendshapes();
            playBackwards = settings.PlayBackwards;
        }

        void InitializeBodyShapeBlendshapes() {
            for (int betaIndex = 0; betaIndex < SMPLConstants.BodyShapeBetaCount; betaIndex++) {
                float scaledBeta = ScaleBlendshapeWeightFromBlenderToUnity(rawBodyShapeWeightBetas[betaIndex]);
                meshRenderer.SetBlendShapeWeight(betaIndex, scaledBeta);
            }
        }

        public void PlayCurrentFrame() {
            if (Finished) return;
            if (!started) {
                startTime = Time.time;
                started = true;
            }
            
            float elapsedTime = Time.time - startTime;

            ResampledFrame resampledFrame;
            if (playBackwards)
                resampledFrame =
                    new BackwardsResampleFrame(elapsedTime, sourceTotalFrameCount, requiredDuration);
            else resampledFrame = new ForwardsResampledFrame(elapsedTime, sourceTotalFrameCount, requiredDuration) ;
            

            if (resampledFrame.IsLastFrame) {
                Finished = true;
                return;
            }

            Vector3 translationThisFrame = GetTranslationAtFrame(resampledFrame);
            Quaternion[] posesThisFrame = GetPosesAtFrame(resampledFrame);
            
            boneModifier.UpdateBoneRotations(posesThisFrame, translationThisFrame);
            UpdatePoseDependentBlendShapes(posesThisFrame);
            
            
        }


        Vector3 GetTranslationAtFrame(ResampledFrame resampledFrame) {
            if (resampledFrame.IsFirstFrame) return translations[0];
            
            //Debug.Log($"finished: {Finished} FrameINdex {resampledFrame.FrameBeforeThis} count: {translations.Length}");
            Vector3 translationAtFrameBeforeThis = translations[resampledFrame.FrameBeforeThis];
            
            if (resampledFrame.IsLastFrame) return translationAtFrameBeforeThis;
            
            Vector3 translationAtFrameAfterThis = translations[resampledFrame.FrameAfterThis];
            
            Vector3 resampledTranslation = Vector3.Lerp(translationAtFrameBeforeThis, 
                                                        translationAtFrameAfterThis, 
                                                        resampledFrame.PercentageElapsedSinceLastFrame);
            return resampledTranslation;
        }

        

        /// <summary>
        /// Populate an array with rotations of each joint at thisFrame. 
        /// </summary>
        Quaternion[] GetPosesAtFrame(ResampledFrame resampledFrame) {
            
            Quaternion[] posesThisFrame = new Quaternion[SMPLConstants.JointCount];
            
            for (int jointIndex = 0; jointIndex < SMPLConstants.JointCount; jointIndex++) {
                
                if (resampledFrame.IsFirstFrame) posesThisFrame[jointIndex] = allPoses[0, jointIndex];
                else if (resampledFrame.IsLastFrame) posesThisFrame[jointIndex] = allPoses[resampledFrame.FrameBeforeThis, jointIndex];
                else {
                    Quaternion rotationAtFrameBeforeThis = allPoses[resampledFrame.FrameBeforeThis, jointIndex];
                    Quaternion rotationAtFrameAfterThis = allPoses[resampledFrame.FrameAfterThis, jointIndex];
                    posesThisFrame[jointIndex] = Quaternion.Slerp(rotationAtFrameBeforeThis, rotationAtFrameAfterThis,
                                                                  resampledFrame.PercentageElapsedSinceLastFrame);
                }
            }
            return posesThisFrame;
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
