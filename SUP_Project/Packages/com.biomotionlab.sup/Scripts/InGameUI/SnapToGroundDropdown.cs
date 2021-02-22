using JetBrains.Annotations;
using Playback;
using SMPLModel;
using TMPro;
using UnityEngine;
using Utilities;

namespace InGameUI {
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
