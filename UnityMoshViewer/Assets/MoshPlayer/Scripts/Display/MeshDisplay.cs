using System;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.Display {
    public class MeshDisplay : MonoBehaviour
    {
        
        MoshCharacter moshCharacter;

        public Material overwriteMaterial;
        MeshDisplayState   MeshDisplayState   => moshCharacter.CharacterSettings.MeshDisplayState;
        MeshDisplaySettings MeshDisplayOptions => moshCharacter.CharacterSettings.MeshDisplayOptions;

        void OnEnable() {
            moshCharacter = GetComponent<MoshCharacter>();
        }

        // Update is called once per frame
        void Update() {
            if (overwriteMaterial != null) {
                moshCharacter.SkinnedMeshRender.material = overwriteMaterial;
            }

            switch (MeshDisplayState) {
                case MeshDisplayState.On:
                    moshCharacter.SkinnedMeshRender.material = MeshDisplayOptions.Opaque;
                    if (overwriteMaterial != null) {
                        moshCharacter.SkinnedMeshRender.material = overwriteMaterial;
                    }
                    moshCharacter.SkinnedMeshRender.enabled = true;
                    break;
                case MeshDisplayState.SemiTransparent:
                    moshCharacter.SkinnedMeshRender.material = MeshDisplayOptions.SemiTransparent;
                    moshCharacter.SkinnedMeshRender.enabled = true;
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
