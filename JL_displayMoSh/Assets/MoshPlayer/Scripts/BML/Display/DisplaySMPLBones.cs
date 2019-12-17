using System;
using System.Collections.Generic;
using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {

    /// <summary>
    /// Adapted from https://forum.unity.com/threads/rendering-bones.34863/ 
    /// </summary>
    public class DisplaySMPLBones : MonoBehaviour {

        SMPLSettings settings;
        Transform pelvisBone;
  
        void SetupBones(Transform parent) {

            List<BoneRenderer> createdBoneRenderers = new List<BoneRenderer>();
            foreach (Transform bone in parent) {
                if (!Bones.IsBone(bone)) continue;
                BoneRenderer newBone = Instantiate(settings.DisplaySettings.BonePrefab);
                newBone.Init(parent, bone, settings);
                createdBoneRenderers.Add(newBone);
                SetupBones(bone);
            }

            foreach (BoneRenderer createdBoneRenderer in createdBoneRenderers) {
                createdBoneRenderer.SetupParent();
            }
        }

        public void Init(MoshCharacter moshCharacter, SMPLSettings settingsFile) {
            settings = settingsFile;
            FindRootBone(moshCharacter.gameObject.transform);
            if (pelvisBone == null) throw new NullReferenceException("Can't find Pelvis bone for rendering");

            SetupBones(pelvisBone);
        }

        void FindRootBone(Transform parentTransform) {
            foreach (Transform bone in parentTransform) {
                if (bone.name == Bones.Pelvis) {
                    pelvisBone = bone;
                }

                FindRootBone(bone);
            }
        }
    }
}