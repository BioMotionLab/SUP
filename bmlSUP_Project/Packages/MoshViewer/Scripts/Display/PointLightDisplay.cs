using SMPLModel;
using UnityEngine;

namespace Display {
    
    /// <summary>
    /// This renders point lights (spheres) for the joints of a body.
    /// </summary>
    public class PointLightDisplay : MonoBehaviour {
        
        SkinnedMeshRenderer meshRenderer;

        [SerializeField]
        PointLight PointLightPrefab = default;

        SMPLCharacter smplCharacter;
        public bool DisplayPointLights => smplCharacter.DisplaySettings.DisplayPointLights == PointLightDisplayState.On;

        GameObject pointLightContainer;

        void OnEnable() {
            smplCharacter = GetComponent<SMPLCharacter>();
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
            newPointLight.AttachBone(smplCharacter, this, parent);
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
