using System;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    
    public class DisplaySMPLPointLights : MonoBehaviour {
        
        SMPLSettings settings;

        public void Init(MoshCharacter moshCharacter, SMPLSettings settingsFile) {
            
            settings = settingsFile;
            SkinnedMeshRenderer skinnedMeshRenderer = moshCharacter.SkinnedMeshRender;
            
            foreach (Transform bone in skinnedMeshRenderer.bones) {

            
                GameObject newPointLight = CreateNewPointLight(bone);
                SetDifferentMaterialsPerSideIfNeeded(bone, newPointLight);
            }
           
        }

        GameObject CreateNewPointLight(Transform bone) {
            GameObject newPointLight = Instantiate(settings.DisplaySettings.PointLightPrefab, bone, false);
            newPointLight.name = $"PointLight for {bone.name}";
            newPointLight.transform.localPosition = Vector3.zero;
            newPointLight.transform.localScale = new Vector3(settings.DisplaySettings.PointLightDisplaySize,
                                                             settings.DisplaySettings.PointLightDisplaySize,
                                                             settings.DisplaySettings.PointLightDisplaySize);
            return newPointLight;
        }

        void SetDifferentMaterialsPerSideIfNeeded(Transform bone, GameObject pointLight) {
            if (!settings.DisplaySettings.DrawSidesDifferentColors) return;
            
            
            MeshRenderer meshRenderer = pointLight.GetComponent<MeshRenderer>();

            SideOfBody side = Bones.GetSideOfBody(bone.name);
            switch (side) {
                case SideOfBody.Left:
                    meshRenderer.sharedMaterial = settings.DisplaySettings.LeftSideMaterial;
                    break;
                case SideOfBody.Right:
                    meshRenderer.sharedMaterial = settings.DisplaySettings.RightSideMaterial;
                    break;
                case SideOfBody.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
