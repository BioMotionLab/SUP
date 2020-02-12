using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class Playback {
        
        readonly int   sourceFPS;
        readonly int   sourceTotalFrameCount;
        readonly float sourceDuration;
        
        float requiredDuration;
        
        float startTime;

        public bool Started  = false;
        public bool Finished = false;


        readonly PlaybackOptions playbackOptions;

        public Playback(int sourceTotalFrameCount,
                        int sourceFPS, PlaybackOptions playbackOptions) {
            this.sourceFPS = sourceFPS;
            this.sourceTotalFrameCount = sourceTotalFrameCount;
            this.playbackOptions = playbackOptions;
            sourceDuration = this.sourceTotalFrameCount / (float) this.sourceFPS;
            requiredDuration = sourceDuration / playbackOptions.DisplaySpeed;
        }

        public void Start() {
            startTime = Time.time;
            Started = true;
        }

        public ResampledFrame GetResampledFrame() {
            
            float elapsedTime = Time.time - startTime;
            
            ResampledFrame resampledFrame;

            if (playbackOptions.PlayBackwards) {
                resampledFrame = new BackwardsResampleFrame(elapsedTime, sourceTotalFrameCount, requiredDuration);
            }
            else {
                resampledFrame = new ForwardsResampledFrame(elapsedTime, sourceTotalFrameCount, requiredDuration);
            }
            
            
            if (resampledFrame.IsLastFrame) {
                //TODO loop
                Finished = true;
            }

            return resampledFrame;
        }
        
    }
}