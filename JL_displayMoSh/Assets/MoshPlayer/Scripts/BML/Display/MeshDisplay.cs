using System;
using System.Collections;
using System.Collections.Generic;
using MoshPlayer.Scripts.BML.Display;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;
using UnityEngine.Serialization;

public class MeshDisplay : MonoBehaviour
{
    
    [SerializeField]
    MeshDisplayState MeshDisplayState;
    
    MoshCharacter moshCharacter;

    [SerializeField]
    Material SemiTransparentMaterial = default;

    [SerializeField]
    Material OpaqueMaterial;

    void OnEnable() {
        moshCharacter = GetComponent<MoshCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (MeshDisplayState) {
            case MeshDisplayState.On:
                moshCharacter.SkinnedMeshRender.material = OpaqueMaterial;
                break;
            case MeshDisplayState.SemiTransparent:
                moshCharacter.SkinnedMeshRender.material = SemiTransparentMaterial;
                break;
            case MeshDisplayState.Off:
                moshCharacter.SkinnedMeshRender.enabled = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
