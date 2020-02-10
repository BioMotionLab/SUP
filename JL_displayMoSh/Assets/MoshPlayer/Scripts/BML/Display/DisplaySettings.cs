using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    [CreateAssetMenu]
    public class DisplaySettings : ScriptableObject {
        
        [Header("General")]
        
        [SerializeField]
        public bool DrawSidesDifferentColors = default;

        [SerializeField]
        public Material LeftSideMaterial = default;

        [SerializeField]
        public Material RightSideMaterial = default;
        
        
        [Header("Point Light Display")]
        
        [SerializeField]
        public float PointLightDisplaySize = 0.08f;
        
        [SerializeField]
        public bool ExcludeFeetForPointLights = true;
        
        [Header("Bone Display")]
        
        [SerializeField]
        public float BoneWidth = 0.04f;

        [SerializeField]
        [Range(0,5)]
        public float DisplaySpeed = 1f;

        [SerializeField]
        public bool PlayBackwards = false;
        
    }
}