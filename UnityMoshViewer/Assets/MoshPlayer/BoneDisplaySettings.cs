using UnityEngine;
// ReSharper disable InconsistentNaming

namespace MoshPlayer {
    [CreateAssetMenu(fileName = "Bone Display Settings", menuName = "new Bone Display Settings", order = 0)]
    public class BoneDisplaySettings : ScriptableObject {
        public float BoneWidth = 0.04f;
        public bool DrawSidesDifferentColors = default;
        public Material LeftSideMaterial = default;
        public Material RightSideMaterial = default;
    }
}