using System;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    public class MeshDisplay : MonoBehaviour
    {
        
        MoshCharacter moshCharacter;

        MeshDisplayState   MeshDisplayState   => moshCharacter.DisplayOptions.MeshDisplayState;
        MeshDisplayOptions MeshDisplayOptions => moshCharacter.DisplayOptions.MeshDisplayOptions;

        void OnEnable() {
            moshCharacter = GetComponent<MoshCharacter>();
        }

        // Update is called once per frame
        void Update()
        {
            switch (MeshDisplayState) {
                case MeshDisplayState.On:
                    moshCharacter.SkinnedMeshRender.material = MeshDisplayOptions.Opaque;
                    break;
                case MeshDisplayState.SemiTransparent:
                    moshCharacter.SkinnedMeshRender.material = MeshDisplayOptions.SemiTransparent;
                    break;
                case MeshDisplayState.Off:
                    moshCharacter.SkinnedMeshRender.enabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
