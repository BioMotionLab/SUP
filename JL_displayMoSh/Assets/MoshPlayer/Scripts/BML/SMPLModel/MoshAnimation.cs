using System;
using System.Text;
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

        IndividualizedBody individualizedBody;
        
        float startTime;
        bool started = false;
        float requiredDuration;
        bool playBackwards;
        public readonly ModelDefinition Model;
        CharacterPoser characterPoser;


        public MoshAnimation(ModelDefinition model, Gender gender,
                             int             sourceTotalFrameCount,
                             int             sourceFPS,
                             float[]         rawBodyShapeWeightBetas,
                             Vector3[]       translations,
                             Quaternion[,]   allPoses) {
            Gender = gender;
            this.Model = model;
            this.sourceFPS = sourceFPS;
            this.sourceTotalFrameCount = sourceTotalFrameCount;
            sourceDuration = this.sourceTotalFrameCount / (float) this.sourceFPS;
            
            
            this.rawBodyShapeWeightBetas = rawBodyShapeWeightBetas;
            this.translations = translations;
            this.allPoses = allPoses;
            Debug.Log(gender);

        }

        public void AttachSkin(SkinnedMeshRenderer skinnedMeshRendererToAttach, SMPLSettings settings) {
            SkinnedMeshRenderer meshRenderer = skinnedMeshRendererToAttach;

            individualizedBody = meshRenderer.GetComponent<IndividualizedBody>();
            individualizedBody.SetupBodyWithBetas(rawBodyShapeWeightBetas);

            characterPoser = meshRenderer.gameObject.GetComponent<CharacterPoser>();
            if (characterPoser == null) throw new NullReferenceException("Can't find CharacterPoser component");

            requiredDuration = sourceDuration / settings.DisplaySpeed;
            playBackwards = settings.PlayBackwards;
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
            
            characterPoser.UpdateBoneRotations(posesThisFrame);
            characterPoser.UpdateTranslation(translationThisFrame);
            
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
            
            Quaternion[] posesThisFrame = new Quaternion[Model.JointCount];
            
            for (int jointIndex = 0; jointIndex < Model.JointCount; jointIndex++) {
                
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


    }
}
