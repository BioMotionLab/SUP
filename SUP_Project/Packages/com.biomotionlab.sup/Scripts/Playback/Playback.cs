using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.Playback {
    public class Playback {
        
        readonly int   sourceTotalFrameCount;
        readonly float sourceDuration;
        
        float lastUpdateTime;
        public bool Started  = false;
        public bool Finished = false;


        bool paused = false;
        
        readonly PlaybackSettings playbackSettings;
        float elapsedTime;
        float playbackSpeed = 1;
        readonly AnimationControlEvents controlEvents;

        public Playback(int sourceTotalFrameCount,
                        int sourceFPS, PlaybackSettings playbackSettings, AnimationControlEvents animationControlEvents) {
            int actualFPS = sourceFPS;
            if (actualFPS <= 0) {
                actualFPS = 60;
                Debug.Log("Defaulting fps");
            }

            this.sourceTotalFrameCount = sourceTotalFrameCount;
            this.playbackSettings = playbackSettings;
            sourceDuration = this.sourceTotalFrameCount / (float) actualFPS;

            PlaybackEventSystem.OnPauseToggleEvent += TogglePause;
            PlaybackEventSystem.OnBroadcastDisplaySpeed += UpdatePlaybackSpeed;

            controlEvents = animationControlEvents;
            controlEvents.OnUserFrameSelect += JumpToFrame;
            
        }

        void JumpToFrame(float frame) {
            elapsedTime = (frame / sourceTotalFrameCount) * sourceDuration;
            //Debug.Log($"frame: {frame} Jumped to time {elapsedTime}");
        }

        void UpdatePlaybackSpeed(float updatedPlaybackSpeed) {
            playbackSpeed = updatedPlaybackSpeed;
        }

        void TogglePause(bool pauseState) {
            paused = pauseState;
        }

        public void Start() {
            Started = true;
            lastUpdateTime = Time.time;
            elapsedTime = 0;
            
            controlEvents.BroadcastTotalFrames(sourceTotalFrameCount);
            PlaybackEventSystem.OnStopAllAnimations += Finish;
            //Debug.Log($"total frames: {sourceTotalFrameCount}");
        }

        public ResampledFrame GetResampledFrame() {

            float signedPlaybackSpeed = playbackSpeed;
            if (playbackSettings.PlayBackwards) {
                signedPlaybackSpeed = -signedPlaybackSpeed;
            }
            if (!paused) elapsedTime += (Time.time - lastUpdateTime) * signedPlaybackSpeed;
            lastUpdateTime = Time.time;
            
            ResampledFrame resampledFrame = new ForwardsResampledFrame(elapsedTime, sourceTotalFrameCount, sourceDuration);
            controlEvents.BroadcastCurrentFrame(resampledFrame.Frame);
            //Debug.Log($"totalframes: {sourceTotalFrameCount}, current frame: {resampledFrame.Frame}");
            
            if (resampledFrame.IsLastFrame) {
                if (playbackSettings.Loop) elapsedTime = 0;
                else Finish();
            }

            return resampledFrame;
        }

        public void Finish() {
            Finished = true;
            //Debug.Log("playback ended");
            controlEvents.BroadCastAnimationEnded();
            PlaybackEventSystem.OnPauseToggleEvent -= TogglePause;
            PlaybackEventSystem.OnBroadcastDisplaySpeed -= UpdatePlaybackSpeed;
            PlaybackEventSystem.OnStopAllAnimations -= Finish;
        }
    }
}