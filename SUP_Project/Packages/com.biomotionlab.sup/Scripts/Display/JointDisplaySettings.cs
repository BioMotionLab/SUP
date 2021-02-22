using System;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable InconsistentNaming

namespace Display {
    [Serializable]
    public class JointDisplaySettings {
        [FormerlySerializedAs("PointLightDisplaySize")] public float JointDisplaySize = 0.01f;
        public bool DrawSidesDifferentColors;
        public Material LeftSideMaterial;
        public Material RightSideMaterial;
    }
}