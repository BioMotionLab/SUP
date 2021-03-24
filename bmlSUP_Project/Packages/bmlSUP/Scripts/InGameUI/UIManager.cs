using Playback;
using UnityEngine;

namespace InGameUI {
    public class UIManager : MonoBehaviour
    {
    
        [SerializeField] GameObject UI = default;
        [SerializeField] KeyCode toggleKey = KeyCode.U;

        bool keyboardControlsEnabled = true;
        
        void OnEnable()
        {
            if (UI != null) UI.SetActive(true);
            Debug.Log(Format.Log($"To toggle UI, press the {toggleKey} key"));
            KeyboardControlEvents.OnEnableKeyboardControls += EnableKeyboard;
            KeyboardControlEvents.OnDisableKeyboardControls += DisableKeyboard;
        }

        void DisableKeyboard() {
            keyboardControlsEnabled = false;
        }

        void EnableKeyboard() {
            keyboardControlsEnabled = true;
        }

        // Update is called once per frame
        void Update() {
            if (keyboardControlsEnabled && Input.GetKeyDown(toggleKey)) {
                UI.SetActive(!UI.activeSelf);
            }
        }
    }
}
