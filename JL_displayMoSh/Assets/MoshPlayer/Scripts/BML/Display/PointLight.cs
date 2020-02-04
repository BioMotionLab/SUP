using System;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    [ExecuteInEditMode]
    public class PointLight : MonoBehaviour {
        Transform linkedBone;
        SMPLDisplaySettings displaySettings;
        MeshRenderer meshRenderer;
        PointLightRenderer displaySMPLPointLights;


        void OnEnable() {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void AttachBone(PointLightRenderer displaySMPLPointLights, Transform bone,
                               SMPLDisplaySettings    displaySettings) {
            linkedBone = bone;
            this.displaySettings = displaySettings;
            this.displaySMPLPointLights = displaySMPLPointLights;
            name = $"PointLight for {bone.name}";
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(displaySettings.PointLightDisplaySize,
                                             displaySettings.PointLightDisplaySize,
                                             displaySettings.PointLightDisplaySize);
            SetDifferentMaterialsPerSideIfNeeded(bone);
        }

        void Update() {
            if (linkedBone != null) {
                transform.position = linkedBone.position;
                meshRenderer.enabled = displaySMPLPointLights.DisplayPointLights;
            }
            else {
                meshRenderer.enabled = false;
            }
            
        }
        
        void SetDifferentMaterialsPerSideIfNeeded(Transform bone) {
            if (!displaySettings.DrawSidesDifferentColors) return;

            SideOfBody side = Bones.GetSideOfBody(bone.name);
            switch (side) {
                case SideOfBody.Left:
                    meshRenderer.sharedMaterial = displaySettings.LeftSideMaterial;
                    break;
                case SideOfBody.Right:
                    meshRenderer.sharedMaterial = displaySettings.RightSideMaterial;
                    break;
                case SideOfBody.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}