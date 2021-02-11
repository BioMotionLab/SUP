using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.SMPLModel;
using MoshPlayer.Scripts.Utilities;
using TMPro;


namespace MoshPlayer.Scripts.InGameUI {
    public class SnapToGroundDropdown : MonoBehaviour
    {
        TMP_Dropdown                   tmpDropdown;
        EnumDropdown<GroundSnapType> enumDropDown;
 
        void OnEnable() {
            tmpDropdown = GetComponent<TMP_Dropdown>();
            enumDropDown = new EnumDropdown<GroundSnapType>();
        }
 
        void Start() {
            if (tmpDropdown == null) return;
            enumDropDown?.PopulateOptions(tmpDropdown);
        }
 
        [PublicAPI]
        public void DropdownIndexChanged(int index) {
            if (tmpDropdown == null) return;
            GroundSnapType snapType = enumDropDown.EnumFrom(index);
            PlaybackEventSystem.ChangeSnapToGround(snapType);
        }
 
 
    }
}
