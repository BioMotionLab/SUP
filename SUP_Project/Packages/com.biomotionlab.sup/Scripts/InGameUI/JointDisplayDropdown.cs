using Display;
using JetBrains.Annotations;
using Playback;
using TMPro;
using UnityEngine;
using Utilities;

namespace InGameUI {
    public class JointDisplayDropdown : MonoBehaviour
    {
        TMP_Dropdown                         tmpDropdown;
        EnumDropdown<JointDisplayState> enumDropDown;

        void OnEnable() {
            tmpDropdown = GetComponent<TMP_Dropdown>();
            enumDropDown = new EnumDropdown<JointDisplayState>();
        }

        void Start() {
            if (tmpDropdown == null) return;
            enumDropDown?.PopulateOptions(tmpDropdown, JointDisplayState.Off);
        }

        [PublicAPI]
        public void DropdownIndexChanged(int index) {
            if (tmpDropdown == null) return;
            JointDisplayState jointDisplayState = enumDropDown.EnumFrom(index);
            PlaybackEventSystem.JointDisplayStateChanged(jointDisplayState);
        }


    }
}