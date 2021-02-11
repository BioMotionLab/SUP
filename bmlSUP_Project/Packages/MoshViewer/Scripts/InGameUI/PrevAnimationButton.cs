using JetBrains.Annotations;
using Playback;
using UnityEngine;

namespace InGameUI {
    
    public class PrevAnimationButton : MonoBehaviour
    {
        [PublicAPI]
        public void PlayPreviousAnimation() {
            PlaybackEventSystem.GoToPreviousAnimation();
        }
    }
}