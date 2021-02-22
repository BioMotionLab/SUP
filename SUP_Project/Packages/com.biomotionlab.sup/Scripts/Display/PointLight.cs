using System;
using SMPLModel;
using UnityEngine;

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
                if (smplCharacter != null) {
                    if (smplCharacter.DisplaySettings != null) {
                        return smplCharacter.DisplaySettings.PointLightDisplaySettings;
                    }
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
        
        public void AttachBone(SMPLCharacter attachedSmplCharacter, PointLightDisplay attachedPointLightDisplay, 
                               Transform bone) {
            this.smplCharacter = attachedSmplCharacter;
            linkedBone = bone;
            this.pointLightDisplay = attachedPointLightDisplay;
            name = $"PointLight for {bone.name}";
            Transform cachedTransform = transform;
            cachedTransform.localPosition = Vector3.zero;
            cachedTransform.localScale = new Vector3(Settings.PointLightDisplaySize,
                                             Settings.PointLightDisplaySize,
                                             Settings.PointLightDisplaySize);
            
            
        }

        void LateUpdate() {
            if (linkedBone != null) {
                transform.position = linkedBone.position;
                meshRenderer.enabled = pointLightDisplay.DisplayPointLights;

                if (pointLightDisplay.DisplayPointLights) {
                    this.transform.localScale = new Vector3(
                        Settings.PointLightDisplaySize,
                        Settings.PointLightDisplaySize,
                        Settings.PointLightDisplaySize);
                    if (Settings.DrawSidesDifferentColors) ColorBySideOfBody(linkedBone);
                }
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