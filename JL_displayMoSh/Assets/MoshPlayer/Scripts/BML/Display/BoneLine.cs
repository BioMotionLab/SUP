using System;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;
// ReSharper disable ParameterHidesMember

namespace MoshPlayer.Scripts.BML.Display {
    /// <summary>
    /// Draws a boneDisplay between two joints.
    /// </summary>
    public class BoneLine : MonoBehaviour {

        LineRenderer lineRenderer;

        Transform childBone;
        Transform parentBone;

        DisplaySettings settings;
        BoneDisplay boneDisplay;

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
        /// <param name="boneDisplay"></param>
        /// <param name="parentBone"></param>
        /// <param name="childBone"></param>
        /// <param name="settings"></param>
        public void Init(BoneDisplay boneDisplay, Transform parentBone, Transform childBone, DisplaySettings settings) {
            this.boneDisplay = boneDisplay;
            this.parentBone = parentBone;
            this.childBone = childBone;
            name = $"BoneLine {this.parentBone.name} to {this.childBone.name}";
            this.settings = settings;
            
            if (settings.DrawSidesDifferentColors) DrawSidesDifferentColors();
        }

        void Update() {
            UpdateBoneLine();
            ShowOrHideBasedOnSettings();
        }

        void UpdateBoneLine() {
            Vector3[] positions = {childBone.position, parentBone.position};
            lineRenderer.SetPositions(positions);
            lineRenderer.startWidth = settings.BoneWidth;
            lineRenderer.endWidth = settings.BoneWidth;
        }

        void ShowOrHideBasedOnSettings() {
            lineRenderer.enabled = boneDisplay.DisplayBones;
        }

        void DrawSidesDifferentColors() {
            SideOfBody sideOfBody = Bones.GetSideOfBody(childBone.name);
            switch (sideOfBody) {
                case SideOfBody.Left:
                    lineRenderer.material = settings.LeftSideMaterial;
                    break;
                case SideOfBody.Right:
                    lineRenderer.material = settings.RightSideMaterial;
                    break;
                case SideOfBody.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
