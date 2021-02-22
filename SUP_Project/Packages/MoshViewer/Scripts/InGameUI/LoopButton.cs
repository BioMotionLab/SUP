using MoshPlayer.Scripts.Playback;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class LoopButton : MonoBehaviour
    {
        public void ChangeLoopState(bool loopState) {
            PlaybackEventSystem.ChangeLoopState(loopState);
        }
    }
}
