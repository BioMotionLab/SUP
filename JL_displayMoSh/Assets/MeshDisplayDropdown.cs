using JetBrains.Annotations;
using MoshPlayer.Scripts.BML.Display;
using TMPro;
using UnityEngine;

public class MeshDisplayDropdown : MonoBehaviour
{
    TMP_Dropdown tmpDropdown;
    EnumDropdown<MeshDisplayState> enumDropDown;

    void OnEnable() {
        tmpDropdown = GetComponent<TMP_Dropdown>();
        enumDropDown = new EnumDropdown<MeshDisplayState>();
    }

    void Start() {
        enumDropDown.PopulateOptions(tmpDropdown);
    }

    [PublicAPI]
    public void DropdownIndexChanged(int index) {
        MeshDisplayState meshDisplayState = enumDropDown.EnumFrom(index);
        PlaybackEventSystem.MeshDisplayStateChanged(meshDisplayState);
    }


}