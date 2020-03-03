using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.Display {

    /// <summary>
    /// Adapted from https://forum.unity.com/threads/rendering-bones.34863/ 
    /// </summary>
    public class BoneDisplay : MonoBehaviour {

        Transform pelvisBone;
        
        [SerializeField]
        public BoneLine BonePrefab;

        SkinnedMeshRenderer skinnedMeshRenderer;

        GameObject boneDisplayContainer;
        MoshCharacter moshCharacter;
        public bool DisplayBones => moshCharacter.DisplayOptions.DisplayBones == BoneDisplayState.On;
        public BoneDisplayOptions BoneDisplayOptions => moshCharacter.DisplayOptions.BoneDisplayOptions;
        
        void OnEnable() {
            moshCharacter = GetComponent<MoshCharacter>();
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            
            SetupBones(skinnedMeshRenderer.bones[moshCharacter.Model.PelvisIndex]);
        }

        void SetupBones(Transform pelvisBoneToSetup) {
            CreateContainerIfNeeded();
            CreateBoneDisplays(pelvisBoneToSetup);
        }

        void CreateBoneDisplays(Transform parent) {
            foreach (Transform child in parent) {
                if (!Bones.IsBone(child)) continue;
                BoneLine newBone = Instantiate(BonePrefab, boneDisplayContainer.transform);
                newBone.Init(this, parent, child, BoneDisplayOptions);
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