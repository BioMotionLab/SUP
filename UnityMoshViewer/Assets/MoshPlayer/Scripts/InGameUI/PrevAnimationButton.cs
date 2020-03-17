using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class PrevAnimationButton : MonoBehaviour
    {
        [PublicAPI]
        public void PlayPrevAnimation() {
            PlaybackEventSystem.GoToPreviousAnimation();
        }
    }
}