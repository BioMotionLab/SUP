using UnityEngine;

namespace MoshPlayer.Scripts.BML {
    
    public class DisplaySMPLPointLights : MonoBehaviour {
        
        public void Init(MoshCharacter moshCharacter, SMPLSettings settings) {
            
            SkinnedMeshRenderer skinnedMeshRenderer = moshCharacter.SkinnedMeshRender;
            foreach (Transform bone in skinnedMeshRenderer.bones) {
                if (bone == skinnedMeshRenderer.rootBone) continue; // don't want to add a light to root. 
                
                if (settings.DisplaySettings.ExcludeFeetForPointLights && (bone.name == SMPLConstants.LeftFootBone || bone.name == SMPLConstants.RightFootBone))
                    continue;
            
                GameObject newPointLight = Instantiate(settings.DisplaySettings.PointLightPrefab, bone, false);
                newPointLight.name = $"PointLight for {bone.name}";
                newPointLight.transform.localPosition = Vector3.zero;
                newPointLight.transform.localScale = new Vector3(settings.DisplaySettings.PointLightDisplaySize,
                                                                 settings.DisplaySettings.PointLightDisplaySize,
                                                                 settings.DisplaySettings.PointLightDisplaySize);
                
                MeshRenderer meshRenderer = newPointLight.GetComponent<MeshRenderer>();
                if (settings.DisplaySettings.DrawSidesDifferentColors) {
                    if (bone.name.Contains(SMPLConstants.LeftBonePrefix)) {
                        meshRenderer.sharedMaterial = settings.DisplaySettings.LeftSideMaterial;
                    }

                    if (bone.name.Contains(SMPLConstants.RightBonePrefix)) {
                        meshRenderer.sharedMaterial = settings.DisplaySettings.RightSideMaterial;
                    }
                }
            }
           
        }

    }
}
