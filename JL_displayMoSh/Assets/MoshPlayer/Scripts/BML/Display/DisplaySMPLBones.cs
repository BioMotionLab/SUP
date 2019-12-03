using System;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    
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
        
        public void Init(MoshCharacter moshCharacter, SMPLSettings settingsFile) {
            settings = settingsFile;
            boneContainer = new GameObject();
            boneContainer.transform.SetParent(moshCharacter.gameObject.transform);
            boneContainer.transform.localPosition = Vector3.zero;
            FindRootBone(moshCharacter.gameObject.transform);
            if (pelvisBone == null) throw new NullReferenceException("Can't find Pelvis bone for rendering");
            
            SetupBones(pelvisBone);
        }

        void FindRootBone(Transform parentTransform) {
            foreach (Transform child in parentTransform) {
                if (child.name == SMPLConstants.PelvisBone) {
                    pelvisBone = child;
                }
                FindRootBone(child);
            }
        }
    }
}