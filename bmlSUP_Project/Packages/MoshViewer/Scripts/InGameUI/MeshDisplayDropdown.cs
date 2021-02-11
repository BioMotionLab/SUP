using Display;
using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.Utilities;
using TMPro;
using UnityEngine;

namespace MoshPlayer.Scripts.InGameUI {
    public class MeshDisplayDropdown : MonoBehaviour
    {
        TMP_Dropdown                   tmpDropdown;
        EnumDropdown<MeshDisplayState> enumDropDown;
 
        void OnEnable() {
            tmpDropdown = GetComponent<TMP_Dropdown>();
            enumDropDown = new EnumDropdown<MeshDisplayState>();
        }
 
        void Start() {
            if (tmpDropdown == null) return;
            enumDropDown?.PopulateOptions(tmpDropdown);
        }
 
        [PublicAPI]
        public void DropdownIndexChanged(int index) {
            if (tmpDropdown == null) return;
            MeshDisplayState meshDisplayState = enumDropDown.EnumFrom(index);
            PlaybackEventSystem.MeshDisplayStateChanged(meshDisplayState);
        }
 
 
    }
}