using System;
using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

// ReSharper disable ParameterHidesMember

namespace MoshPlayer.Scripts.Display {
    
    /// <summary>
    /// A single point light for displaying a joint's location.
    /// </summary>
    [ExecuteInEditMode]
    public class PointLight : MonoBehaviour {
        Transform linkedBone;
        PointLightDisplayOptions displaySettings;
        MeshRenderer meshRenderer;
        PointLightDisplay pointLightDisplay;
        
        void OnEnable() {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        public void AttachBone(PointLightDisplay pointLightDisplay, 
                               Transform bone,
                               PointLightDisplayOptions    displaySettings) {
            linkedBone = bone;
            this.displaySettings = displaySettings;
            this.pointLightDisplay = pointLightDisplay;
            name = $"PointLight for {bone.name}";
            var cachedTransform = transform;
            cachedTransform.localPosition = Vector3.zero;
            cachedTransform.localScale = new Vector3(displaySettings.PointLightDisplaySize,
                                             displaySettings.PointLightDisplaySize,
                                             displaySettings.PointLightDisplaySize);
            
            if (displaySettings.DrawSidesDifferentColors) ColorBySideOfBody(bone);
        }

        void LateUpdate() {
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
                    meshRenderer.sharedMaterial = displaySettings.LeftSideMaterial;
                    break;
                case SideOfBody.Right:
                    meshRenderer.sharedMaterial = displaySettings.RightSideMaterial;
                    break;
                case SideOfBody.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}