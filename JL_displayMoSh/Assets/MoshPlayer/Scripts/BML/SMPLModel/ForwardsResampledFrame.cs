using System;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public interface ResampledFrame {
        bool IsFirstFrame { get; }
        bool IsLastFrame { get; }

        int FrameBeforeThis { get; }
        int FrameAfterThis { get; }
        float PercentageElapsedSinceLastFrame { get; }
    }

    public class ForwardsResampledFrame : ResampledFrame {
        readonly int totalFrameCount;
        readonly float decimalFrameIndex;
        
        public ForwardsResampledFrame(float elapsedTime, int totalFrameCount, float duration) {
            this.totalFrameCount = totalFrameCount;
            float proportionComplete = elapsedTime / duration;
            decimalFrameIndex = proportionComplete * totalFrameCount;
            FrameBeforeThis = Math.Max(0, Mathf.FloorToInt(decimalFrameIndex));
            FrameAfterThis = Math.Min(totalFrameCount, Mathf.CeilToInt(decimalFrameIndex));
            PercentageElapsedSinceLastFrame = decimalFrameIndex - FrameBeforeThis;
        }
        
        public bool IsFirstFrame => Math.Abs(decimalFrameIndex) < 0.0001f;
        public bool IsLastFrame => FrameAfterThis >= totalFrameCount;
        public int FrameBeforeThis { get; }

        public int FrameAfterThis { get; }

        public float PercentageElapsedSinceLastFrame { get; }
    }

    public class BackwardsResampleFrame : ResampledFrame {
        readonly        int   totalFrameCount;
        public int FrameBeforeThis { get; }
        public int FrameAfterThis { get; }
        public float PercentageElapsedSinceLastFrame { get; }
        readonly        float decimalFrameIndex;
        
        public BackwardsResampleFrame(float elapsedTime, int totalFrameCount, float duration) {
            this.totalFrameCount = totalFrameCount;
            float proportionComplete = 1 - elapsedTime / duration;
            decimalFrameIndex = proportionComplete * totalFrameCount;
            FrameBeforeThis = Math.Max(0, Mathf.FloorToInt(decimalFrameIndex));
            FrameAfterThis = Math.Min(totalFrameCount, Mathf.CeilToInt(decimalFrameIndex));
            PercentageElapsedSinceLastFrame = decimalFrameIndex - FrameBeforeThis;
        }
        
        public bool IsFirstFrame => FrameAfterThis >= totalFrameCount;
        public bool IsLastFrame => Math.Abs(decimalFrameIndex) < 1f;
    }
}