using System;
using SMPLModel;
using UnityEngine;

// ReSharper disable ParameterHidesMember

namespace Display {
    /// <summary>
    /// Draws a boneDisplay between two joints.
    /// </summary>
    public class BoneLine : MonoBehaviour {
        
        [SerializeField]
        BoneDisplaySettings defaultBoneDisplaySettings = default;
        
        
        LineRenderer lineRenderer;

        Transform childBone;
        Transform parentBone;
        
        BoneDisplay boneDisplay;
        SMPLCharacter smplCharacter;

        BoneDisplaySettings Settings {
            get {
                if (smplCharacter.DisplaySettings != null && smplCharacter.DisplaySettings.BoneDisplaySettings != null)
                    return smplCharacter.DisplaySettings.BoneDisplaySettings;
                
                return defaultBoneDisplaySettings;
            }
        }

        void OnEnable() {
            lineRenderer = GetComponent<LineRenderer>();
            DisableFirstFrame();
        }

        /// <summary>
        /// Needs to start disabled to avoid artifacts while it awaits initialization.
        /// </summary>
        void DisableFirstFrame() {
            lineRenderer.enabled = false;
        }

        /// <summary>
        /// Replaces constructor for MonoBehaviour
        /// </summary>
        /// <param name="smplCharacter"></param>
        /// <param name="boneDisplay"></param>
        /// <param name="parentBone"></param>
        /// <param name="childBone"></param>
        public void Init(SMPLCharacter smplCharacter, BoneDisplay boneDisplay, Transform parentBone, Transform childBone) {
            this.smplCharacter = smplCharacter;
            this.boneDisplay = boneDisplay;
            this.parentBone = parentBone;
            this.childBone = childBone;
            name = $"BoneLine {this.parentBone.name} to {this.childBone.name}";
            if (Settings.DrawSidesDifferentColors) DrawSidesDifferentColors();
        }
        
        

        void LateUpdate() {
            UpdateBoneLine();
            ShowOrHideBasedOnSettings();
        }

        void UpdateBoneLine() {
            Vector3[] positions = {childBone.position, parentBone.position};
            lineRenderer.SetPositions(positions);
            lineRenderer.startWidth = Settings.BoneWidth;
            lineRenderer.endWidth = Settings.BoneWidth;
        }

        void ShowOrHideBasedOnSettings() {
            lineRenderer.enabled = boneDisplay.DisplayBones;
        }

        void DrawSidesDifferentColors() {
            SideOfBody sideOfBody = Bones.GetSideOfBody(childBone.name);
            switch (sideOfBody) {
                case SideOfBody.Left:
                    lineRenderer.material = Settings.LeftSideMaterial;
                    break;
                case SideOfBody.Right:
                    lineRenderer.material = Settings.RightSideMaterial;
                    break;
                case SideOfBody.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
