using JetBrains.Annotations;
using MoshPlayer.Scripts.BML.Display;
using TMPro;
using UnityEngine;

public class BoneDisplayDropdown : MonoBehaviour
{
    TMP_Dropdown                   tmpDropdown;
    EnumDropdown<BoneDisplayState> enumDropDown;

    void OnEnable() {
        tmpDropdown = GetComponent<TMP_Dropdown>();
        enumDropDown = new EnumDropdown<BoneDisplayState>();
    }

    void Start() {
        enumDropDown.PopulateOptions(tmpDropdown, BoneDisplayState.Off);
    }

    [PublicAPI]
    public void DropdownIndexChanged(int index) {
        BoneDisplayState boneDisplayState = enumDropDown.EnumFrom(index);
        PlaybackEventSystem.BoneDisplayStateChanged(boneDisplayState);
    }


}