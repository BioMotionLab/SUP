using System;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    public class BoneRenderer : MonoBehaviour {

        LineRenderer lineRenderer;

        public Transform child;
        public Transform parent;

        SMPLSettings settings;
    
        void Awake() {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
        }

        public void Init(Transform boneParent, Transform boneChild, SMPLSettings settingsFile) {
            parent = boneParent;
            child = boneChild;
            name = $"BoneRenderer {parent.name} to {child.name}";
            settings = settingsFile;
            
            SetDifferentSideColorsIfNeeded();
        }

        void Update() {
            Vector3[] positions = {child.position, parent.position};
            lineRenderer.SetPositions(positions);
            lineRenderer.startWidth = settings.DisplaySettings.BoneWidth;
            lineRenderer.endWidth = settings.DisplaySettings.BoneWidth;
            
            EnsureLineRendererEnabled();
        }

        /// <summary>
        /// Starts disabled to avoid artifacts the first frame when it's huge and not initialized. Need to enable it when ready to display.
        /// </summary>
        void EnsureLineRendererEnabled() {
            lineRenderer.enabled = true;
        }

        void SetDifferentSideColorsIfNeeded() {
            if (!settings.DisplaySettings.DrawSidesDifferentColors) return;

            SideOfBody sideOfBody = Bones.GetSideOfBody(child.name);
            switch (sideOfBody) {
                case SideOfBody.Left:
                    lineRenderer.material = settings.DisplaySettings.LeftSideMaterial;
                    break;
                case SideOfBody.Right:
                    lineRenderer.material = settings.DisplaySettings.RightSideMaterial;
                    break;
                case SideOfBody.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetupParent() {
            transform.SetParent(child);
        }
    }
}
