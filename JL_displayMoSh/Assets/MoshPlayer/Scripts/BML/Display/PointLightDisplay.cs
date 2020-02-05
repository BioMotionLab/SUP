using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    
    /// <summary>
    /// This renders point lights (spheres) for the joints of a body.
    /// </summary>
    public class PointLightDisplay : MonoBehaviour {
        
        [SerializeField]
        DisplaySettings DisplaySettings = default;

        SkinnedMeshRenderer meshRenderer;

        [SerializeField]
        PointLight PointLightPrefab;

        [SerializeField]
        public bool DisplayPointLights = true;

        GameObject pointLightContainer;

        void OnEnable() {
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
            newPointLight.AttachBone(this, parent, DisplaySettings);
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
