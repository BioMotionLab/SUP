using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class LoadNewAnimation : MonoBehaviour
    {

        public void LoadNewAnimationSet() {
            PlaybackEventSystem.StopPlayingAllAnimations();
            PlaybackEventSystem.LoadNewAnimations();
        }
    }
}
