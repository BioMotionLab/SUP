using System;
using System.Collections;
using System.Collections.Generic;
using InGameUI;
using UnityEngine;

public class FreezeCameraButton : MonoBehaviour {

    bool frozen = false;
    [SerializeField] FlyCamera flyCamera;
    void OnEnable() {
        frozen = false;
    }

    public void ToggleFreezeState() {
        frozen = !frozen;
        flyCamera.enabled = !frozen;
    }
}
