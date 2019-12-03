using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    
    public class DisplaySMPLPointLights : MonoBehaviour {
        
        SMPLSettings settings;

        public void Init(MoshCharacter moshCharacter, SMPLSettings settingsFile) {
            
            settings = settingsFile;
            SkinnedMeshRenderer skinnedMeshRenderer = moshCharacter.SkinnedMeshRender;
            
            foreach (Transform bone in skinnedMeshRenderer.bones) {
                if (bone == skinnedMeshRenderer.rootBone) continue; //skip root 
                bool isFootBone = (bone.name == SMPLConstants.LeftFootBone || bone.name == SMPLConstants.RightFootBone);
                if (settings.DisplaySettings.ExcludeFeetForPointLights && isFootBone)
                    continue;
            
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
            
            if (bone.name.Contains(SMPLConstants.LeftBonePrefix)) {
                meshRenderer.sharedMaterial = settings.DisplaySettings.LeftSideMaterial;
            }
            if (bone.name.Contains(SMPLConstants.RightBonePrefix)) {
                meshRenderer.sharedMaterial = settings.DisplaySettings.RightSideMaterial;
            }
        }
    }
}
