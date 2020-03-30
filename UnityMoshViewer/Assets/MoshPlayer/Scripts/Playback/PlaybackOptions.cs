using System;
// ReSharper disable InconsistentNaming

namespace MoshPlayer.Scripts.Playback {
    [Serializable]
    public class PlaybackOptions {
        
        public bool PlayBackwards = false;
        public int FallbackFPS = 60;
        
        public bool Loop;
    }
}