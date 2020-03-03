using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class ShowHidePanelButton : MonoBehaviour {

    [FormerlySerializedAs("ReviewPanel")]
    [SerializeField]
    GameObject LinkedPanel = default;

    [FormerlySerializedAs("startEnabled")]
    [SerializeField]
    bool StartEnabled = default;

    public void OnEnable() {
        SetState(StartEnabled);
    }

    [PublicAPI]
    public void ToggleShow() {
        SetState(!LinkedPanel.activeSelf);
    }

    void SetState(bool state) {
        LinkedPanel.SetActive(state);
    }

}
