using JetBrains.Annotations;
using Playback;
using UnityEngine;

namespace InGameUI {
    public class RestartAnimationsButton : MonoBehaviour
    {
        [PublicAPI]
        public void RestartAnimations() {
            PlaybackEventSystem.RestartAnimations();
        }
    }
}