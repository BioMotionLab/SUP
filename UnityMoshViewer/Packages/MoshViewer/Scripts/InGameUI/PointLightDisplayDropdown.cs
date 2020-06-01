using JetBrains.Annotations;
using MoshPlayer.Scripts.Display;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.Utilities;
using TMPro;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class PointLightDisplayDropdown : MonoBehaviour
    {
        TMP_Dropdown                         tmpDropdown;
        EnumDropdown<PointLightDisplayState> enumDropDown;

        void OnEnable() {
            tmpDropdown = GetComponent<TMP_Dropdown>();
            enumDropDown = new EnumDropdown<PointLightDisplayState>();
        }

        void Start() {
            if (tmpDropdown == null) return;
            enumDropDown?.PopulateOptions(tmpDropdown, PointLightDisplayState.Off);
        }

        [PublicAPI]
        public void DropdownIndexChanged(int index) {
            if (tmpDropdown == null) return;
            PointLightDisplayState pointLightDisplayState = enumDropDown.EnumFrom(index);
            PlaybackEventSystem.PointLightDisplayStateChanged(pointLightDisplayState);
        }


    }
}