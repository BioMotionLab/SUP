using System;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEditor;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    
    [ExecuteInEditMode]
    public class PointLightRenderer : MonoBehaviour {
        
        [SerializeField]
        SMPLDisplaySettings DisplaySettings = default;

        SkinnedMeshRenderer meshRenderer;

        [SerializeField]
        PointLight PointLightPrefab;

        [SerializeField]
        public bool DisplayPointLights = true;

        GameObject pointLightContainer;

        void OnEnable() {
            if (meshRenderer == null) meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (pointLightContainer == null) {
                CreatePointLights();
            }
        }

        [ContextMenu("Create PointLights")]
        void CreatePointLights() {
            pointLightContainer = new GameObject {name = "PointLight Container"};
            pointLightContainer.transform.parent = transform;
            CreatePointLightsInBoneHierarchy(meshRenderer.bones[0]);
        }

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
            DestroyPointLights();
        }

        void DestroyPointLights() {
            DestroyImmediate(pointLightContainer);
        }
    }
    
}
