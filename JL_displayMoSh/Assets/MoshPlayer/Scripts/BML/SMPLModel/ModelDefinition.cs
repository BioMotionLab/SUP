using System;
using MoshPlayer.Scripts.BML.FileLoaders;
using MoshPlayer.Scripts.ThirdParty.SimpleJSON;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    
    [CreateAssetMenu]
    public class ModelDefinition : ScriptableObject {

        [SerializeField]
        string modelName = default;
       

        [SerializeField]
        int bodyShapeBetaCount = 10;

        [SerializeField]
        int jointCount = 24;

        [SerializeField]
        int poseDependentBlendshapeCount = 207;

        [SerializeField]
        JSONModelKeys jsonKeys = new JSONModelKeys();

        [SerializeField] 
        MoshCharacterComponent MaleCharacterPrefab = default;
        
        [SerializeField] 
        MoshCharacterComponent FemaleCharacterPrefab = default;

        
        MoshCharacter GetCharacterPrefab(Gender gender) {
            switch (gender) {
                case Gender.Female: 
                    return FemaleCharacterPrefab;
                case Gender.Male:
                    return MaleCharacterPrefab;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
        }
        
        public string ModelName => modelName;
        public float PoseBlendshapeScalingFactor      => 1f / poseBlendShapeScalingFactor;

        public float ShapeBlendShapeScalingFactor => 1f / shapeBlendShapeScalingFactor;
        
       
        public float UnityBlendShapeScaleFactor => unityBlendShapeScaleFactor;
        public int   BodyShapeBetaCount           => bodyShapeBetaCount;
        public int   JointCount                   => jointCount;
        public int   PoseDependentBlendshapeCount => poseDependentBlendshapeCount;

        public JSONModelKeys JsonKeys => jsonKeys;
        public bool RotateToUnityCoords = false;

        public int PelvisIndex = 0;

        public MoshCharacter CreateNewCharacter(string characterName, Gender gender) {
            MoshCharacter genderedPrefab = GetCharacterPrefab(gender);
            GameObject newCharacter = Instantiate(genderedPrefab.gameObject);
            newCharacter.name = characterName;
            MoshCharacter newMoshCharacter = newCharacter.GetComponent<MoshCharacter>();
            return newMoshCharacter;
        }

        [SerializeField]
        bool skipFirstPose = false;

        public bool SkipFirstPose => skipFirstPose;
       
        
        public TextAsset RegressorFile;


        [SerializeField]
        float unityBlendShapeScaleFactor = 100f;

        [SerializeField]
        float poseBlendShapeScalingFactor = 5f;

        [SerializeField]
        float shapeBlendShapeScalingFactor = 5f;

        
        public bool AddShapeBlendshapes;

        [FormerlySerializedAs("AddPoseBlenshapes")]
        public bool AddPoseBlendshapes;

        public bool Animate;
        
        public Vector3 OffsetErrorInFBXBetweenRigAndMesh;
    }
}