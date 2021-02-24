using System;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Display {
    [Serializable]
    public class BoneDisplaySettings {
        public float BoneWidth = 0.005f;
        public bool DrawSidesDifferentColors = default;
        public Material LeftSideMaterial = default;
        public Material RightSideMaterial = default;
    }
}