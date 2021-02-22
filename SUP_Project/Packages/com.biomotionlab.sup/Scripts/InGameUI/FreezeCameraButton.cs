using InGameUI;
using UnityEngine;

public class FreezeCameraButton : MonoBehaviour {

    bool frozen;
    
    // Start is called before the first frame update
    void Start() {
        frozen = false;
        KeyboardControlEvents.EnableKeyboardControls();
    }

    public void ToggleCameraFreeze() {
        frozen = !frozen;
        if (frozen) {
            KeyboardControlEvents.DisableKeyboardControls();
        }
        else {
            KeyboardControlEvents.EnableKeyboardControls();
        }
    }
}
