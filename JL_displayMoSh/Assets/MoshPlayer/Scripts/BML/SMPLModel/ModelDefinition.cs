using System;
using MoshPlayer.Scripts.BML.FileLoaders;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    
    [CreateAssetMenu]
    public class ModelDefinition : ScriptableObject {

        [SerializeField]
        string modelName = default;
        
        [SerializeField]
        float blendShapeScalingFactor = 5f;

        [SerializeField]
        int bodyShapeBetaCount = 10;

        [SerializeField]
        int jointCount = 24;

        [SerializeField]
        int poseDependentBlendshapeCount = 207;

        [SerializeField]
        JSONModelKeys jsonKeys = default;

        
        [SerializeField]
        Mesh MaleMeshPrefab = default;
    
        [SerializeField]
        Mesh FemaleMeshPrefab = default;
        
        public Mesh GetMeshPrefab(Gender gender) {
            switch (gender) {
                case Gender.Female: 
                    return FemaleMeshPrefab;
                case Gender.Male:
                    return MaleMeshPrefab;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
        }
        
        public string ModelName => modelName;
        public float BlendshapeScalingFactor      => 1f / blendShapeScalingFactor;
        public int   BodyShapeBetaCount           => bodyShapeBetaCount;
        public int   JointCount                   => jointCount;
        public int   PoseDependentBlendshapeCount => poseDependentBlendshapeCount;

        public JSONModelKeys JsonKeys => jsonKeys;
    }
}