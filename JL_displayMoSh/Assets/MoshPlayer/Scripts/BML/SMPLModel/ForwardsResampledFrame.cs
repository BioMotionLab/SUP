using System;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public interface ResampledFrame {
        bool IsFirstFrame { get; }
        bool IsLastFrame { get; }

        int FrameBeforeThis { get; }
        int FrameAfterThis { get; }
        float PercentageElapsedSinceLastFrame { get; }

        float Frame { get; }
    }

    public class ForwardsResampledFrame : ResampledFrame {
        
        public float Frame => decimalFrameIndex;
        
        readonly int totalFrameCount;
        readonly float decimalFrameIndex;
        
        public ForwardsResampledFrame(float elapsedTime, int totalFrameCount, float duration) {
            this.totalFrameCount = totalFrameCount;
            float proportionComplete = elapsedTime / duration;
            decimalFrameIndex = proportionComplete * totalFrameCount;
            FrameBeforeThis = Mathf.Clamp(Mathf.FloorToInt(decimalFrameIndex), 0, totalFrameCount-1);
            FrameAfterThis = Mathf.Clamp(Mathf.CeilToInt(decimalFrameIndex), 0, totalFrameCount-1);
            PercentageElapsedSinceLastFrame = decimalFrameIndex - FrameBeforeThis;
        }
        
        public bool IsFirstFrame => Math.Abs(decimalFrameIndex) < 0.0001f;
        public bool IsLastFrame => FrameAfterThis >= totalFrameCount-1;
        public int FrameBeforeThis { get; }

        public int FrameAfterThis { get; }

        public float PercentageElapsedSinceLastFrame { get; }
    }
    
}