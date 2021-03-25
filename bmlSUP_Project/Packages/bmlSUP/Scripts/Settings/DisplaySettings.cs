using Display;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable InconsistentNaming

namespace Settings {
    [CreateAssetMenu(fileName = "New DisplaySettings Asset", menuName = Menu.AssetMenu + "New DisplaySettings Asset")]
    public class DisplaySettings : ScriptableObject {
        
        public BoneDisplayState DisplayBones;
        public PointLightDisplayState DisplayPointLights;
        [FormerlySerializedAs("MeshDisplayState")] public MeshDisplayState DisplayMeshAs;
        
        public MeshDisplaySettings MeshDisplaySettings = default;
        public BoneDisplaySettings BoneDisplaySettings = default;
        [FormerlySerializedAs("PointLightDisplaySettings")] public JointSphereDisplaySettings jointSphereDisplaySettings = default;
    }
}