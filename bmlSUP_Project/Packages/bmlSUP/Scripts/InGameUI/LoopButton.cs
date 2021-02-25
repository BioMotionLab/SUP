using Playback;
using UnityEngine;

namespace InGameUI {
    public class LoopButton : MonoBehaviour
    {
        public void ChangeLoopState(bool loopState) {
            PlaybackEventSystem.ChangeLoopState(loopState);
        }
    }
}
