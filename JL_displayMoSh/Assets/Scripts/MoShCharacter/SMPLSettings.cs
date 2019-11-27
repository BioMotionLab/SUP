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
}