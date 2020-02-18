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

        BoneDisplayOptions boneDisplayOptions;
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
        /// <param animationName="boneDisplay"></param>
        /// <param animationName="parentBone"></param>
        /// <param animationName="childBone"></param>
        /// <param animationName="boneDisplayOptions"></param>
        public void Init(BoneDisplay boneDisplay, Transform parentBone, Transform childBone, BoneDisplayOptions boneDisplayOptions) {
            this.boneDisplay = boneDisplay;
            this.parentBone = parentBone;
            this.childBone = childBone;
            name = $"BoneLine {this.parentBone.name} to {this.childBone.name}";
            this.boneDisplayOptions = boneDisplayOptions;
            
            if (boneDisplayOptions.DrawSidesDifferentColors) DrawSidesDifferentColors();
        }

        void LateUpdate() {
            UpdateBoneLine();
            ShowOrHideBasedOnSettings();
        }

        void UpdateBoneLine() {
            Vector3[] positions = {childBone.position, parentBone.position};
            lineRenderer.SetPositions(positions);
            lineRenderer.startWidth = boneDisplayOptions.BoneWidth;
            lineRenderer.endWidth = boneDisplayOptions.BoneWidth;
        }

        void ShowOrHideBasedOnSettings() {
            lineRenderer.enabled = boneDisplay.DisplayBones;
        }

        void DrawSidesDifferentColors() {
            SideOfBody sideOfBody = Bones.GetSideOfBody(childBone.name);
            switch (sideOfBody) {
                case SideOfBody.Left:
                    lineRenderer.material = boneDisplayOptions.LeftSideMaterial;
                    break;
                case SideOfBody.Right:
                    lineRenderer.material = boneDisplayOptions.RightSideMaterial;
                    break;
                case SideOfBody.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
