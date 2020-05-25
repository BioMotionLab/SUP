using UnityEngine;
// ReSharper disable InconsistentNaming

namespace MoshPlayer {
    [CreateAssetMenu(fileName = "PointLight Display Settings", menuName = "new PointLight Display Settings", order = 0)]
    public class PointLightDisplaySettings : ScriptableObject {
        public float PointLightDisplaySize = 0.02f;
        public bool DrawSidesDifferentColors;
        public Material LeftSideMaterial;
        public Material RightSideMaterial;
        
    }
}