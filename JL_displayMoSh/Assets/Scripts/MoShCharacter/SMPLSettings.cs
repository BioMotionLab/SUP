using System;
using UnityEngine;

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

    public JointCalculator MaleJointCalculator => new JointCalculatorFromJSON(MaleJointRegressorFile).BuildWithSettings(this);

    [SerializeField]
    TextAsset FemaleJointRegressorFile = default;
    public JointCalculator FemaleJointCalculator => new JointCalculatorFromJSON(FemaleJointRegressorFile).BuildWithSettings(this);

    [SerializeField]
    public bool HideMeshWhenFinished = true;

    public Mesh GetMeshPrefab(Gender gender) {
        switch (gender) {
            case Gender.Female: 
                return FemaleMeshPrefab;
            case Gender.MALE:
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