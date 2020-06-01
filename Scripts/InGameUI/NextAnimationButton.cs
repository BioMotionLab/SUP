using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class NextAnimationButton : MonoBehaviour
    {


        [PublicAPI]
        public void PlayNextAnimation() {
            PlaybackEventSystem.GoToNextAnimation();
        }
    }
}
