using System;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class MoshAnimation {
        
        readonly Vector3[]     translations;
        readonly Quaternion[,] allPoses;
        readonly float[]       rawBodyShapeWeightBetas;
        
        
        public Gender Gender { get; }
        public bool Finished => playback.Finished;
        
        IndividualizedBody individualizedBody;
        public readonly ModelDefinition Model;
        CharacterPoser characterPoser;
        readonly Playback playback;
        AnimationControlEvents animationControlEvents;


        public MoshAnimation(ModelDefinition model, PlaybackOptions playbackOptions,
                             Gender gender,
                             int             sourceTotalFrameCount,
                             int             sourceFPS,
                             float[]         rawBodyShapeWeightBetas,
                             Vector3[]       translations,
                             Quaternion[,]   allPoses) {
            Gender = gender;
            Model = model;
            
            animationControlEvents = new AnimationControlEvents();
            playback = new Playback(sourceTotalFrameCount, sourceFPS, playbackOptions, animationControlEvents);
            
            this.rawBodyShapeWeightBetas = rawBodyShapeWeightBetas;
            this.translations = translations;
            this.allPoses = allPoses;

        }

        public void AttachSkin(SkinnedMeshRenderer skinnedMeshRendererToAttach, PlaybackOptions playbackOptions) {
            SkinnedMeshRenderer meshRenderer = skinnedMeshRendererToAttach;

            individualizedBody = meshRenderer.GetComponent<IndividualizedBody>();
            individualizedBody.UpdateBodyWithBetas(rawBodyShapeWeightBetas);

            characterPoser = meshRenderer.gameObject.GetComponent<CharacterPoser>();
            if (characterPoser == null) throw new NullReferenceException("Can't find CharacterPoser component");
            
        }

        

        public void PlayCurrentFrame() {
            if (playback.Finished) return;
            if (!playback.Started) {
                AnimationControlEvents.BroadCastAnimationStarted(this, animationControlEvents);
                playback.Start();
            }

            ResampledFrame resampledFrame = playback.GetResampledFrame();
            
            Vector3 translationThisFrame = GetTranslationAtFrame(resampledFrame);
            Quaternion[] posesThisFrame = GetPosesAtFrame(resampledFrame);
            
            characterPoser.SetPoses(posesThisFrame);
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
        /// PopulateOptions an array with rotations of each joint at thisFrame. 
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
