using Display;
using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.Utilities;
using TMPro;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class BoneDisplayDropdown : MonoBehaviour
    {
        TMP_Dropdown                   tmpDropdown;
        EnumDropdown<BoneDisplayState> enumDropDown;

        void OnEnable() {
            tmpDropdown = GetComponent<TMP_Dropdown>();
            enumDropDown = new EnumDropdown<BoneDisplayState>();
        }

        void Start() {
            if (tmpDropdown == null) return;
            enumDropDown?.PopulateOptions(tmpDropdown, BoneDisplayState.Off);
        }

        [PublicAPI]
        public void DropdownIndexChanged(int index) {
            if (tmpDropdown == null) return;
            BoneDisplayState boneDisplayState = enumDropDown.EnumFrom(index);
            PlaybackEventSystem.BoneDisplayStateChanged(boneDisplayState);
        }


    }
}