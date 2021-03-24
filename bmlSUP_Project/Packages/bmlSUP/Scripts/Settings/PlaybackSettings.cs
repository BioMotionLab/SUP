using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Settings {
    
    [CreateAssetMenu(fileName = "New PlaybackSettings Asset", menuName = Menu.AssetMenu + "New PlaybackSettings Asset", order = 0)]
    public class PlaybackSettings : ScriptableObject {
        
        public bool PlayBackwards = false;
        public int FallbackFPS = 60;
        
        public bool Loop;
        public bool OffsetMultipleAnimations = false;
        public Vector3 OffSetSpacing;
        public bool PlaybackImmediately = true;
    }
}