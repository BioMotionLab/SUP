using JetBrains.Annotations;
using Playback;
using UnityEngine;

namespace InGameUI {
    public class NextAnimationButton : MonoBehaviour
    {


        [PublicAPI]
        public void PlayNextAnimation() {
            PlaybackEventSystem.GoToNextAnimation();
        }
    }
}
