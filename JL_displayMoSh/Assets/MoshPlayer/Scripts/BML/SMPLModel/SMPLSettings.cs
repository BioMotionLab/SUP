using System;
using MoshPlayer.Scripts.BML.Display;
using MoshPlayer.Scripts.BML.FileLoaders;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    [CreateAssetMenu]
    public class SMPLSettings : ScriptableObject {

        [SerializeField]
        GameObject CharacterPrefab = default;
        
        [SerializeField]
        Mesh MaleMeshPrefab = default;
    
        [SerializeField]
        Mesh FemaleMeshPrefab = default;

        [SerializeField]
        TextAsset MaleJointRegressorFile = default;

        public JointCalculator MaleJointCalculator => new JointCalculatorFromJSON(MaleJointRegressorFile).Build();

        [SerializeField]
        TextAsset FemaleJointRegressorFile = default;
        public JointCalculator FemaleJointCalculator => new JointCalculatorFromJSON(FemaleJointRegressorFile).Build();

        [SerializeField]
        [Range(0,5)]
        public float DisplaySpeed = 1f;

        [SerializeField]
        public bool PlayBackwards = false;
        
        [SerializeField]
        public SMPLDisplaySettings DisplaySettings = default;
        
        [SerializeField]
        public bool SnapMeshFeetToGround = true;
        
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

        public MoshCharacterComponent CreateNewCharacter() {
            GameObject newCharacter = Instantiate(CharacterPrefab);
            MoshCharacterComponent newMoshCharacterComponent = newCharacter.GetComponent<MoshCharacterComponent>();
            return newMoshCharacterComponent;
        }
    }
}