using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyboardControlEvents
{
    public delegate void DisableKeyboardControlsEvent();

    public static event DisableKeyboardControlsEvent OnDisableKeyboardControls;

    public static void DisableKeyboardControls() {
        OnDisableKeyboardControls?.Invoke();
    }

    public delegate void EnableKeyboardControlsEvent();

    public static event EnableKeyboardControlsEvent OnEnableKeyboardControls;

    public static void EnableKeyboardControls() {
        OnEnableKeyboardControls?.Invoke();
    }
}
