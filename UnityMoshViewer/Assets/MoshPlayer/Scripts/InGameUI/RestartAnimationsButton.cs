using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class RestartAnimationsButton : MonoBehaviour
    {
        [PublicAPI]
        public void RestartAnimations() {
            PlaybackEventSystem.RestartAnimations();
        }
    }
}