using System;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace MoshPlayer.Scripts.Playback {
    
    [CreateAssetMenu(fileName = "Playback Settings", menuName = "new Playback Settings", order = 0)]
    public class PlaybackSettings : ScriptableObject {
        
        public bool PlayBackwards = false;
        public int FallbackFPS = 60;
        
        public bool Loop;
        public bool OffsetMultipleAnimations = false;
        public Vector3 OffSetSpacing;
        public bool PlaybackImmediately = true;
    }
}