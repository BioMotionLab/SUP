using System;
using SMPLModel;
using UnityEngine;

// ReSharper disable ParameterHidesMember

namespace Display {
    
    /// <summary>
    /// A single point light for displaying a joint's location.
    /// </summary>
    [ExecuteInEditMode]
    public class PointLight : MonoBehaviour {
        

        [SerializeField]
        PointLightDisplaySettings defaultPointlightDisplaySettings = default;

        PointLightDisplaySettings Settings {
            get {
                if (smplCharacter != null && smplCharacter.DisplaySettings != null) {
                    return smplCharacter.DisplaySettings.PointLightDisplaySettings;
                }
                return defaultPointlightDisplaySettings;
            }
        }
        Transform linkedBone;
        MeshRenderer meshRenderer;
        PointLightDisplay pointLightDisplay;
        SMPLCharacter smplCharacter;
        void OnEnable() {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        public void AttachBone(SMPLCharacter smplCharacter, PointLightDisplay pointLightDisplay, 
                               Transform bone) {
            this.smplCharacter = smplCharacter;
            linkedBone = bone;
            this.pointLightDisplay = pointLightDisplay;
            name = $"PointLight for {bone.name}";
            var cachedTransform = transform;
            cachedTransform.localPosition = Vector3.zero;
            cachedTransform.localScale = new Vector3(Settings.PointLightDisplaySize,
                                             Settings.PointLightDisplaySize,
                                             Settings.PointLightDisplaySize);
            
        }

        void LateUpdate() {
            
            if (Settings.DrawSidesDifferentColors) ColorBySideOfBody(linkedBone);
            this.transform.localScale = new Vector3(Settings.PointLightDisplaySize, Settings.PointLightDisplaySize,
                Settings.PointLightDisplaySize);
            
            if (linkedBone != null) {
                transform.position = linkedBone.position;
                meshRenderer.enabled = pointLightDisplay.DisplayPointLights;
            }
            else {
                meshRenderer.enabled = false;
            }
            
        }

        /// <summary>
        /// Colors the sides of body differently
        /// </summary>
        /// <param name="bone"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void ColorBySideOfBody(Transform bone) {
            SideOfBody side = Bones.GetSideOfBody(bone.name);
            switch (side) {
                case SideOfBody.Left:
                    meshRenderer.sharedMaterial = Settings.LeftSideMaterial;
                    break;
                case SideOfBody.Right:
                    meshRenderer.sharedMaterial = Settings.RightSideMaterial;
                    break;
                case SideOfBody.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}