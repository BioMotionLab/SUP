using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML {
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
        public bool CharacterFeetSnapToGround = true;
        
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

        public MoshCharacter CreateNewCharacter() {
            GameObject newCharacter = Instantiate(CharacterPrefab);
            MoshCharacter newMoshCharacter = newCharacter.GetComponent<MoshCharacter>();
            return newMoshCharacter;
        }
    }
}