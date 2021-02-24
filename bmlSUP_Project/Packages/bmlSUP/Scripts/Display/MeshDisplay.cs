using System;
using SMPLModel;
using UnityEngine;

namespace Display {
    public class MeshDisplay : MonoBehaviour
    {
        
        SMPLCharacter smplCharacter;

        public Material overwriteMaterial;
        MeshDisplayState   DisplayMeshAs   => smplCharacter.DisplaySettings.DisplayMeshAs;
        MeshDisplaySettings MeshDisplayOptions => smplCharacter.DisplaySettings.MeshDisplaySettings;

        void OnEnable() {
            smplCharacter = GetComponent<SMPLCharacter>();
        }

        // Update is called once per frame
        void Update() {
            if (overwriteMaterial != null) {
                smplCharacter.SkinnedMeshRender.material = overwriteMaterial;
            }

            switch (DisplayMeshAs) {
                case MeshDisplayState.Opaque:
                    smplCharacter.SkinnedMeshRender.material = MeshDisplayOptions.Opaque;
                    if (overwriteMaterial != null) {
                        smplCharacter.SkinnedMeshRender.material = overwriteMaterial;
                    }
                    smplCharacter.SkinnedMeshRender.enabled = true;
                    break;
                case MeshDisplayState.SemiTransparent:
                    smplCharacter.SkinnedMeshRender.material = MeshDisplayOptions.SemiTransparent;
                    smplCharacter.SkinnedMeshRender.enabled = true;
                    break;
                case MeshDisplayState.Off:
                    smplCharacter.SkinnedMeshRender.enabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
