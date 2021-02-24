using SMPLModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace Display {

    /// <summary>
    /// Adapted from https://forum.unity.com/threads/rendering-bones.34863/ 
    /// </summary>
    public class BoneDisplay : MonoBehaviour {
        
        [SerializeField]
        [FormerlySerializedAs("BonePrefab")]
        public BoneLine bonePrefab;

        SkinnedMeshRenderer skinnedMeshRenderer;

        GameObject boneDisplayContainer;
        SMPLCharacter smplCharacter;
        public bool DisplayBones => smplCharacter.DisplaySettings.DisplayBones == BoneDisplayState.On;
     
        
        void OnEnable() {
            smplCharacter = GetComponent<SMPLCharacter>();
            if (smplCharacter == null) Debug.LogError("Can't find SMPLCharacter");
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            SetupBones(skinnedMeshRenderer.bones[smplCharacter.Model.PelvisIndex]);
        }

        void SetupBones(Transform pelvisBoneToSetup) {
            CreateContainerIfNeeded();
            CreateBoneDisplays(pelvisBoneToSetup);
        }

        void CreateBoneDisplays(Transform parent) {
            foreach (Transform child in parent) {
                if (!Bones.IsBone(child)) continue;
                BoneLine newBone = Instantiate(bonePrefab, boneDisplayContainer.transform);
                newBone.Init(smplCharacter, this, parent, child);
                SetupBones(child);
            }
        }

        void CreateContainerIfNeeded() {
            if (boneDisplayContainer != null) return;
            
            boneDisplayContainer = new GameObject() {name = "BoneDisplay Container"};
            boneDisplayContainer.transform.SetParent(smplCharacter.gameObject.transform);
        }

        void OnDisable() {
            DestroyImmediate(boneDisplayContainer);
        }
    }
}