using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable InconsistentNaming

namespace Display {
    [CreateAssetMenu(fileName = "Display Settings", menuName = "New Display Settings", order = 0)]
    public class DisplaySettings : ScriptableObject {
        
        public BoneDisplayState DisplayBones;
        [FormerlySerializedAs("DisplayJoints")] public JointDisplayState displayJoints;
        [FormerlySerializedAs("MeshDisplayState")] public MeshDisplayState DisplayMeshAs;
        
        public MeshDisplaySettings MeshDisplaySettings = default;
        public BoneDisplaySettings BoneDisplaySettings = default;
        [FormerlySerializedAs("PointLightDisplaySettings")] public JointDisplaySettings jointDisplaySettings = default;
    }
}