using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class ShowHidePanelButton : MonoBehaviour {

    [FormerlySerializedAs("ReviewPanel")]
    [SerializeField]
    GameObject LinkedPanel = default;


    [PublicAPI]
    public void ToggleShow() {
        LinkedPanel.SetActive(!LinkedPanel.activeSelf);
    }

}
