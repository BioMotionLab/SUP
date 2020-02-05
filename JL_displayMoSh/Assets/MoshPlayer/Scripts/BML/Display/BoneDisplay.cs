using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {

    /// <summary>
    /// Adapted from https://forum.unity.com/threads/rendering-bones.34863/ 
    /// </summary>
    public class BoneDisplay : MonoBehaviour {

        DisplaySettings displaySettings;
        Transform pelvisBone;

        [SerializeField]
        public bool DisplayBones = true;

        SkinnedMeshRenderer skinnedMeshRenderer;

        GameObject boneDisplayContainer;
        MoshCharacter moshCharacter;

        void OnEnable() {
            moshCharacter = GetComponent<MoshCharacter>();
            displaySettings = moshCharacter.SettingsMain.DisplaySettings;
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            
            SetupBones(skinnedMeshRenderer.bones[moshCharacter.Model.PelvisIndex]);
        }

        void SetupBones(Transform pelvisBone) {
            CreateContainerIfNeeded();
            CreateBoneDisplays(pelvisBone);
        }

        void CreateBoneDisplays(Transform parent) {
            foreach (Transform child in parent) {
                if (!Bones.IsBone(child)) continue;
                BoneLine newBone = Instantiate(displaySettings.BonePrefab, boneDisplayContainer.transform);
                newBone.Init(this, parent, child, displaySettings);
                SetupBones(child);
            }
        }

        void CreateContainerIfNeeded() {
            if (boneDisplayContainer != null) return;
            
            boneDisplayContainer = new GameObject() {name = "BoneDisplay Container"};
            boneDisplayContainer.transform.SetParent(moshCharacter.gameObject.transform);
        }

        void OnDisable() {
            DestroyImmediate(boneDisplayContainer);
        }
    }
}