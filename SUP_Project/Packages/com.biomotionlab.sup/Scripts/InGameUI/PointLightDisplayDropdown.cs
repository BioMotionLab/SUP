using Display;
using JetBrains.Annotations;
using Playback;
using TMPro;
using UnityEngine;
using Utilities;

namespace InGameUI {
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