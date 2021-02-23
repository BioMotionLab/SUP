using System;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Display {
    [Serializable]
    public class PointLightDisplaySettings {
        public float PointLightDisplaySize = 0.01f;
        public bool DrawSidesDifferentColors;
        public Material LeftSideMaterial;
        public Material RightSideMaterial;
    }
}