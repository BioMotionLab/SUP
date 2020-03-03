using System;

namespace MoshPlayer.Scripts.Playback {
    [Serializable]
    public class PlaybackOptions {
        
        public bool PlayBackwards = false;
        public int FallbackFPS = 120;
        
        //TODO Loop
        public bool Loop;
    }
}