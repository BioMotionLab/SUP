using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    
    [CreateAssetMenu]
    public class ModelDefinition : ScriptableObject {

        [SerializeField]
        string modelName = default;

        [SerializeField]
        int bodyShapeBetaCount = default;

        [SerializeField]
        int jointCount = default;

        [SerializeField]
        int poseDependentBlendshapeCount = default;

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
      
        public int   BodyShapeBetaCount           => bodyShapeBetaCount;
        public int   JointCount                   => jointCount;
        public int   PoseDependentBlendshapeCount => poseDependentBlendshapeCount;

        public JSONModelKeys JsonKeys => jsonKeys;
        public bool RotateToUnityCoords = false;

        public int PelvisIndex = 0;

        public MoshCharacter CreateNewCharacter(string characterName, Gender gender) {
            MoshCharacter genderedPrefab = GetCharacterPrefab(gender);
            GameObject newCharacter = Instantiate(genderedPrefab.gameObject);
            Debug.Log("Instantiating char");
            newCharacter.name = characterName;
            MoshCharacter newMoshCharacter = newCharacter.GetComponent<MoshCharacter>();
            return newMoshCharacter;
        }

        [FormerlySerializedAs("skipFirstPose")]
        [SerializeField]
        // ReSharper disable once InconsistentNaming
        bool firstPoseIsPelvisTranslation = false;

        public bool FirstPoseIsPelvisTranslation => firstPoseIsPelvisTranslation;
        
        [SerializeField]
        TextAsset MaleRegressorFile = default;
        
        [SerializeField]
        TextAsset FemaleRegressorFile = default;

        public TextAsset RegressorFile(Gender gender) {
            switch (gender) {
                case Gender.Male:
                    return MaleRegressorFile;
                case Gender.Female:
                    return FemaleRegressorFile;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
        }

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        float unityBlendShapeScaleFactor = 100f;

        public float UnityBlendShapeScaleFactor => unityBlendShapeScaleFactor;

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        float poseBlendShapeScalingFactor = 1f;

        public float PoseBlendshapeScalingFactor => 1f / poseBlendShapeScalingFactor;

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        float shapeBlendShapeScalingFactor = 1f;

        public float ShapeBlendShapeScalingFactor => 1f / shapeBlendShapeScalingFactor;
    }
}