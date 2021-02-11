using Playback;
using UnityEngine;

namespace InGameUI {
    public class LoadNewAnimation : MonoBehaviour
    {

        public void LoadNewAnimationSet() {
            PlaybackEventSystem.StopPlayingAllAnimations();
            PlaybackEventSystem.LoadNewAnimations();
        }
    }
}
