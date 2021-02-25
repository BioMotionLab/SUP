using System;
using System.Collections;
using System.Collections.Generic;
using InGameUI;
using UnityEngine;

public class FreezeCameraButton : MonoBehaviour {

    bool frozen = false;

    void OnEnable() {
        frozen = false;
    }

    public void ToggleFreezeState() {
        frozen = !frozen;
        if (frozen) KeyboardControlEvents.DisableKeyboardControls();
        else KeyboardControlEvents.EnableKeyboardControls();
    }
}
