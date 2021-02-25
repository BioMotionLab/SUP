using Playback;
using TMPro;
using UnityEngine;

namespace InGameUI {
    public class PlayerProgressText : MonoBehaviour {

        TextMeshProUGUI text;
    
        void OnEnable() {
            text = GetComponent<TextMeshProUGUI>();
            PlaybackEventSystem.OnUpdatePlayerProgress += UpdateText;
        }

        void OnDisable() {
            PlaybackEventSystem.OnUpdatePlayerProgress -= UpdateText;
        }

        void UpdateText(string s) {
            text.text = s;
        }
    }
}
