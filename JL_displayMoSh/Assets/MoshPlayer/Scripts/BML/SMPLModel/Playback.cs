using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class Playback {
        
        readonly int   sourceFPS;
        readonly int   sourceTotalFrameCount;
        readonly float sourceDuration;
        
        float startTime;
        float lastUpdateTime;
        public bool Started  = false;
        public bool Finished = false;


        public bool paused = false;
        
        readonly PlaybackOptions playbackOptions;
        float elapsedTime;
        float playbackSpeed = 1;
        AnimationControlEvents controlEvents;

        public Playback(int sourceTotalFrameCount,
                        int sourceFPS, PlaybackOptions playbackOptions, AnimationControlEvents animationControlEvents) {
            this.sourceFPS = sourceFPS;
            if (sourceFPS <= 0) {
                this.sourceFPS = 60;
                Debug.Log("Defaulting fps");
            }

            this.sourceTotalFrameCount = sourceTotalFrameCount;
            this.playbackOptions = playbackOptions;
            sourceDuration = this.sourceTotalFrameCount / (float) this.sourceFPS;

            PlaybackEventSystem.OnPauseToggleEvent += TogglePause;
            PlaybackEventSystem.OnBroadcastDisplaySpeed += UpdatePlaybackSpeed;

            controlEvents = animationControlEvents;
            controlEvents.OnUserFrameSelect += JumpToFrame;
        }

        void JumpToFrame(float frame) {
            elapsedTime = (frame / sourceTotalFrameCount) * sourceDuration;
            Debug.Log($"frame: {frame} Jumped to time {elapsedTime}");
        }

        void UpdatePlaybackSpeed(float updatedPlaybackSpeed) {
            playbackSpeed = updatedPlaybackSpeed;
        }

        void TogglePause(bool pauseState) {
            paused = pauseState;
        }

        public void Start() {
            startTime = Time.time;
            Started = true;
            elapsedTime = 0;
            
            controlEvents.BroadcastTotalFrames(sourceTotalFrameCount);
            //Debug.Log($"total frames: {sourceTotalFrameCount}");
        }

        public ResampledFrame GetResampledFrame() {

            float signedPlaybackSpeed = playbackSpeed;
            if (playbackOptions.PlayBackwards) {
                signedPlaybackSpeed = -signedPlaybackSpeed;
            }

            if (!paused) elapsedTime += (Time.time - lastUpdateTime) * playbackSpeed;
            lastUpdateTime = Time.time;
            
            ResampledFrame resampledFrame = new ForwardsResampledFrame(elapsedTime, sourceTotalFrameCount, sourceDuration);
            controlEvents.BroadcastCurrentFrame(resampledFrame.Frame);
            //Debug.Log($"totalframes: {sourceTotalFrameCount}, current frame: {resampledFrame.Frame}");
            
            if (resampledFrame.IsLastFrame) {
                //TODO loop
                if (playbackOptions.Loop) elapsedTime = 0;
                else Finish();
            }

            return resampledFrame;
        }

        void Finish() {
            Finished = true;
            controlEvents.BroadCastAnimationEnded();
            PlaybackEventSystem.OnPauseToggleEvent -= TogglePause;
            PlaybackEventSystem.OnBroadcastDisplaySpeed -= UpdatePlaybackSpeed;
        }
    }
}