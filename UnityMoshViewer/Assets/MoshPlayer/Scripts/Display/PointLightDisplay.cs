using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.Display {
    
    /// <summary>
    /// This renders point lights (spheres) for the joints of a body.
    /// </summary>
    public class PointLightDisplay : MonoBehaviour {
        
        SkinnedMeshRenderer meshRenderer;

        [SerializeField]
        PointLight PointLightPrefab = default;

        MoshCharacter moshCharacter;
        public bool DisplayPointLights => moshCharacter.CharacterSettings.DisplayPointLights == PointLightDisplayState.On;

        GameObject pointLightContainer;

        void OnEnable() {
            moshCharacter = GetComponent<MoshCharacter>();
            if (meshRenderer == null) meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (pointLightContainer == null) {
                SetupPointLights();
            }
        }

        void SetupPointLights() {
            pointLightContainer = new GameObject {name = "PointLight Container"};
            pointLightContainer.transform.parent = transform;
            CreatePointLightsInBoneHierarchy(meshRenderer.bones[0]);
        }

        /// <summary>
        /// Walks down Bone Hierarchy to create linked point lights
        /// </summary>
        /// <param name="parent"></param>
        void CreatePointLightsInBoneHierarchy(Transform parent) {
            PointLight newPointLight = Instantiate(PointLightPrefab, pointLightContainer.transform);
            newPointLight.AttachBone(moshCharacter, this, parent);
            foreach (Transform child in parent) {
                if (Bones.IsBone(child)) {
                    CreatePointLightsInBoneHierarchy(child);
                }
            }
        }

        void OnDisable() {
            DestroyImmediate(pointLightContainer);
        }
    }
    
}
