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
        

        [SerializeField]
        PointLightDisplaySettings defaultPointlightDisplaySettings;

        PointLightDisplaySettings Settings {
            get {
                if (moshCharacter != null && moshCharacter.CharacterSettings != null) {
                    return moshCharacter.CharacterSettings.PointLightDisplaySettings;
                }
                return defaultPointlightDisplaySettings;
            }
        }
        Transform linkedBone;
        MeshRenderer meshRenderer;
        PointLightDisplay pointLightDisplay;
        MoshCharacter moshCharacter;
        void OnEnable() {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        public void AttachBone(MoshCharacter moshCharacter, PointLightDisplay pointLightDisplay, 
                               Transform bone) {
            linkedBone = bone;
            this.pointLightDisplay = pointLightDisplay;
            name = $"PointLight for {bone.name}";
            var cachedTransform = transform;
            cachedTransform.localPosition = Vector3.zero;
            cachedTransform.localScale = new Vector3(Settings.PointLightDisplaySize,
                                             Settings.PointLightDisplaySize,
                                             Settings.PointLightDisplaySize);
            
            if (Settings.DrawSidesDifferentColors) ColorBySideOfBody(bone);
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