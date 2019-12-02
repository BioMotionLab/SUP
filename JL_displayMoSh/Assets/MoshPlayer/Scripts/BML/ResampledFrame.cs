using UnityEngine;

namespace MoshPlayer.Scripts.BML {
    public class ResampledFrame {
        public readonly int   FrameBeforeThis;
        public readonly int   FrameAfterThis;
        public readonly float PercentageElapsedSinceLastFrame;
        readonly        float resampledTotalFrameCount;
        readonly        float duration;

        public ResampledFrame(int thisFrame, int sourceFPS, float resampledTotalFrameCount, float duration) {
            this.duration = duration;
            this.resampledTotalFrameCount = resampledTotalFrameCount;
            float elapsedTimeUpToFrame = GetTimeAtFrame(thisFrame);
            float decimalFrameIndex = sourceFPS * elapsedTimeUpToFrame;
            FrameBeforeThis = Mathf.FloorToInt(decimalFrameIndex);
            FrameAfterThis = Mathf.CeilToInt(decimalFrameIndex);
            PercentageElapsedSinceLastFrame = decimalFrameIndex - FrameBeforeThis;
            
        }
        
        /// <summary>
        /// Get the time, in seconds since start of animation, at a specified thisFrameAsDecimal.
        /// </summary>
        float GetTimeAtFrame(int frame) {
            float percentComplete = frame / resampledTotalFrameCount;
            float timeAtFrame = percentComplete * duration;
            return timeAtFrame;
        }

    }
}