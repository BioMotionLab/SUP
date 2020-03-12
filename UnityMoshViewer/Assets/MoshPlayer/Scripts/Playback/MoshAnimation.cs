using System;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.Playback {
    public class MoshAnimation {
        
        readonly Vector3[]     translations;
        readonly Quaternion[,] allPoses;
        readonly float[]       rawBodyShapeWeightBetas;
        
        
        public Gender Gender { get; }
        public bool Finished => playback.Finished;

        public IndividualizedBody Body {
            get => individualizedBody;
            set => individualizedBody = value;
        }

        IndividualizedBody individualizedBody;
        public readonly ModelDefinition Model;
        readonly PlaybackOptions playbackOptions;
        readonly int sourceTotalFrameCount;
        readonly int sourceFPS;
        CharacterPoser characterPoser;
        Playback playback;
        AnimationControlEvents animationControlEvents;
        public readonly string AnimationName;


        public MoshAnimation(ModelDefinition model, PlaybackOptions playbackOptions,
                             Gender gender,
                             int             sourceTotalFrameCount,
                             int             sourceFPS,
                             float[]         rawBodyShapeWeightBetas,
                             Vector3[]       translations,
                             Quaternion[,]   allPoses,
                             string animationName) {
            Gender = gender;
            Model = model;
            this.playbackOptions = playbackOptions;
            this.sourceTotalFrameCount = sourceTotalFrameCount;
            this.sourceFPS = sourceFPS;


            this.rawBodyShapeWeightBetas = rawBodyShapeWeightBetas;
            this.translations = translations;
            this.allPoses = allPoses;
            AnimationName = animationName;

            Setup();
        }

        void Setup() {
            animationControlEvents = new AnimationControlEvents();
            playback = new Playback(sourceTotalFrameCount, sourceFPS, playbackOptions, animationControlEvents);
        }

        public void AttachSkin(SkinnedMeshRenderer skinnedMeshRendererToAttach, PlaybackOptions playbackOptions) {
            Reset();
            SkinnedMeshRenderer meshRenderer = skinnedMeshRendererToAttach;
            
            characterPoser = meshRenderer.gameObject.GetComponent<CharacterPoser>();
            if (characterPoser == null) throw new NullReferenceException("Can't find CharacterPoser component");
            
            Body = meshRenderer.GetComponent<IndividualizedBody>();
            Body.UpdateBodyWithBetas(rawBodyShapeWeightBetas);
        }

        public void Reset() {
            Setup();
        }


        public void PlayCurrentFrame() {
            bool firstUpdate = false;
            if (playback.Finished) return;
            if (!playback.Started) {
                AnimationControlEvents.BroadCastAnimationStarted(this, animationControlEvents);
                playback.Start();
                firstUpdate = true;
            }
            
            if (firstUpdate) {
                SetPosesAndTranslationForFirstFrame(FirstFrameGuaranteed.Instance);
            }
            else {
                SetPosesAndTranslationFromFrame(playback.GetResampledFrame());
            }
        }

        void SetPosesAndTranslationFromFrame(ResampledFrame resampledFrame) {
            Quaternion[] posesThisFrame = GetPosesAtFrame(resampledFrame);
            Vector3 translationThisFrame = GetTranslationAtFrame(resampledFrame);
            characterPoser.SetPoses(posesThisFrame);
            characterPoser.SetTranslation(translationThisFrame);
        }

        /// <summary>
        /// Ensures Poser initialization is done on the first real frame every time
        /// it's played back, so that it's not dependent on frame rendering time.
        /// </summary>
        void SetPosesAndTranslationForFirstFrame(ResampledFrame resampledFrame) {
            characterPoser.NotifyFirstFrame();
            SetPosesAndTranslationFromFrame(resampledFrame);
            characterPoser.ForceUpdate();
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

        public void End() {
            playback.Finish();
        }
    }
}
