using System;
using SMPLModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace Display {
    
    /// <summary>
    /// A single sphere displaying a joint's location.
    /// </summary>
    [ExecuteInEditMode]
    public class JointSphere : MonoBehaviour {
        

        [FormerlySerializedAs("defaultPointlightDisplaySettings")] [SerializeField]
        JointDisplaySettings defaultJointDisplaySettings = default;

        JointDisplaySettings Settings {
            get {
                if (smplCharacter != null) {
                    if (smplCharacter.DisplaySettings != null) {
                        return smplCharacter.DisplaySettings.jointDisplaySettings;
                    }
                }
                return defaultJointDisplaySettings;
            }
        }
        Transform linkedBone;
        MeshRenderer meshRenderer;
        JointDisplay jointDisplay;
        SMPLCharacter smplCharacter;
        void OnEnable() {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        public void AttachBone(SMPLCharacter attachedSmplCharacter, JointDisplay attachedJointDisplay, 
                               Transform bone) {
            this.smplCharacter = attachedSmplCharacter;
            linkedBone = bone;
            this.jointDisplay = attachedJointDisplay;
            name = $"JointSphere for {bone.name}";
            Transform cachedTransform = transform;
            cachedTransform.localPosition = Vector3.zero;
            cachedTransform.localScale = new Vector3(Settings.JointDisplaySize,
                                             Settings.JointDisplaySize,
                                             Settings.JointDisplaySize);
            
            
        }

        void LateUpdate() {
            if (linkedBone != null) {
                transform.position = linkedBone.position;
                meshRenderer.enabled = jointDisplay.DisplayJoints;

                if (jointDisplay.DisplayJoints) {
                    this.transform.localScale = new Vector3(
                        Settings.JointDisplaySize,
                        Settings.JointDisplaySize,
                        Settings.JointDisplaySize);
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