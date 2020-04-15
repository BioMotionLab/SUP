using System;
using MoshPlayer.Scripts.FileLoaders;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.Playback {
    public class MoshAnimation {
        
        public bool Finished => playback.Finished;

        // ReSharper disable once ConvertToAutoProperty
        public IndividualizedBody Body {
            get => individualizedBody;
            private set => individualizedBody = value;
        }

        public AnimationData Data => data;

        readonly AnimationData data;
        
        IndividualizedBody individualizedBody;
        readonly PlaybackOptions playbackOptions;

        CharacterPoser characterPoser;
        Playback playback;
        AnimationControlEvents animationControlEvents;
        public readonly string AnimationName;


        public MoshAnimation(AnimationData data, PlaybackOptions playbackOptions, string animationName) {
            this.data = data;
            this.playbackOptions = playbackOptions;
            AnimationName = animationName;
            Setup();
        }

        void Setup() {
            animationControlEvents = new AnimationControlEvents();
            playback = new Playback(Data.FrameCount, Data.Fps, playbackOptions, animationControlEvents);
        }

        public void AttachSkin(SkinnedMeshRenderer skinnedMeshRendererToAttach) {
            Reset();
            SkinnedMeshRenderer meshRenderer = skinnedMeshRendererToAttach;
            
            characterPoser = meshRenderer.gameObject.GetComponent<CharacterPoser>();
            if (characterPoser == null) throw new NullReferenceException("Can't find CharacterPoser component");
            
            Body = meshRenderer.GetComponent<IndividualizedBody>();
            Body.UpdateBodyWithBetas(Data.Betas);
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
                //Debug.Log("firstUpdate");
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
            if (resampledFrame.IsFirstFrame) return Data.Translations[0];
            
            Vector3 translationAtFrameBeforeThis = Data.Translations[resampledFrame.FrameBeforeThis];
            
            if (resampledFrame.IsLastFrame) return translationAtFrameBeforeThis;
            
            Vector3 translationAtFrameAfterThis = Data.Translations[resampledFrame.FrameAfterThis];
            
            Vector3 resampledTranslation = Vector3.Lerp(translationAtFrameBeforeThis, 
                                                        translationAtFrameAfterThis, 
                                                        resampledFrame.PercentageElapsedSinceLastFrame);
            return resampledTranslation;
        }


        /// <summary>
        /// PopulateOptions an array with rotations of each joint at thisFrame. 
        /// </summary>
        Quaternion[] GetPosesAtFrame(ResampledFrame resampledFrame) {
            
            Quaternion[] posesThisFrame = new Quaternion[Data.Model.JointCount];
            
            for (int jointIndex = 0; jointIndex < Data.Model.JointCount; jointIndex++) {
                
                if (resampledFrame.IsFirstFrame) posesThisFrame[jointIndex] = Data.Poses[0, jointIndex];
                else if (resampledFrame.IsLastFrame) posesThisFrame[jointIndex] = Data.Poses[resampledFrame.FrameBeforeThis, jointIndex];
                else {
                    Quaternion rotationAtFrameBeforeThis = Data.Poses[resampledFrame.FrameBeforeThis, jointIndex];
                    Quaternion rotationAtFrameAfterThis = Data.Poses[resampledFrame.FrameAfterThis, jointIndex];
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
