using SMPLModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace Display {
    
    /// <summary>
    /// This renders point lights (spheres) for the joints of a body.
    /// </summary>
    public class PointLightDisplay : MonoBehaviour {
        
        SkinnedMeshRenderer meshRenderer;

        [FormerlySerializedAs("PointLightPrefab")] [SerializeField]
        JointSphere jointSpherePrefab = default;

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
            pointLightContainer = new GameObject {name = "JointSphere Container"};
            pointLightContainer.transform.parent = transform;
            CreatePointLightsInBoneHierarchy(meshRenderer.bones[0]);
        }

        /// <summary>
        /// Walks down Bone Hierarchy to create linked point lights
        /// </summary>
        /// <param name="parent"></param>
        void CreatePointLightsInBoneHierarchy(Transform parent) {
            JointSphere newJointSphere = Instantiate(jointSpherePrefab, pointLightContainer.transform);
            newJointSphere.AttachBone(smplCharacter, this, parent);
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
