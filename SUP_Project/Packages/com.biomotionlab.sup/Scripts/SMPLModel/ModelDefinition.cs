using System;
using FileLoaders;
using Playback;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable InconsistentNaming

namespace SMPLModel {
    
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
        H5ModelKeys h5Keys = new H5ModelKeys();
        
        [SerializeField] 
        CharacterComponent MaleCharacterPrefab = default;
        
        [SerializeField] 
        CharacterComponent FemaleCharacterPrefab = default;

        
        SMPLCharacter GetCharacterPrefab(Gender gender) {
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
        public H5ModelKeys H5Keys => h5Keys;
        public bool RotateToUnityCoords = false;

        public int PelvisIndex = 0;

        public SMPLCharacter CreateCharacter(MoshAnimation moshAnimation, int characterIndex) {
            Gender gender = moshAnimation.Data.Gender;
            
            SMPLCharacter genderedPrefab = GetCharacterPrefab(gender);
            if (genderedPrefab == null) throw new NullReferenceException("Gender Prefab is null");
            
            GameObject newCharacter = Instantiate(genderedPrefab.gameObject);
     
            newCharacter.name = $"{gender} Character {characterIndex}";

            SMPLCharacter newSMPLCharacter = newCharacter.GetComponent<SMPLCharacter>();
            newSMPLCharacter.SetIndex(characterIndex);
            
            return newSMPLCharacter;
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