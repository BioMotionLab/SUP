using UnityEngine;

namespace MoshPlayer.Scripts.BML {
    [CreateAssetMenu]
    public class SMPLDisplaySettings : ScriptableObject {
        
        [Header("General")]
        
        [SerializeField]
        public bool DrawSidesDifferentColors = default;

        [SerializeField]
        public Material LeftSideMaterial = default;

        [SerializeField]
        public Material RightSideMaterial = default;
        
        [SerializeField]
        public Material SemiTransparentMaterial = default; 

        
        [Header("Point Light Display")]
        
        [SerializeField]
        public GameObject PointLightPrefab;

        [SerializeField]
        public float PointLightDisplaySize = 0.08f;
        
        [SerializeField]
        public bool ExcludeFeetForPointLights = true;
        
        [Header("Bone Display")]
        
        [SerializeField]
        public float BoneWidth = 0.04f;

        [SerializeField]
        public BoneRenderer BonePrefab;

        
    }
}