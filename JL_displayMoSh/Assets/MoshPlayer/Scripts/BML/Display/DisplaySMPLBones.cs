using System;
using UnityEngine;

namespace MoshPlayer.Scripts.BML {
    
    /// <summary>
    /// Adapted from https://forum.unity.com/threads/rendering-bones.34863/ 
    /// </summary>
    public class DisplaySMPLBones : MonoBehaviour {

        SMPLSettings settings;
        
        GameObject boneContainer;
        Transform pelvisBone;

        void SetupBones(Transform parent) {
            
            foreach (Transform child in parent) {
                
                BoneRenderer newBone = Instantiate(settings.DisplaySettings.BonePrefab, boneContainer.transform);
                newBone.Init(parent, child, settings);
                
                SetupBones(child);
            }
        }
        
        public void Init(MoshCharacter moshCharacter, SMPLSettings settings) {
            this.settings = settings;
            boneContainer = new GameObject();
            boneContainer.transform.SetParent(moshCharacter.gameObject.transform);
            boneContainer.transform.localPosition = Vector3.zero;
            FindRootBone(moshCharacter.gameObject.transform);
            if (pelvisBone == null) throw new NullReferenceException("Can't find Pelvis bone for rendering");
            
            SetupBones(pelvisBone);
        }

        void FindRootBone(Transform transform) {
            foreach (Transform child in transform) {
                if (child.name == SMPLConstants.PelvisBone) {
                    pelvisBone = child;
                }
                FindRootBone(child);
            }
        }
    }
}