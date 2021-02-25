using SMPLModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace Display {
    
    /// <summary>
    /// This renders point lights (spheres) for the joints of a body.
    /// </summary>
    public class JointDisplay : MonoBehaviour {
        
        SkinnedMeshRenderer meshRenderer;

        [FormerlySerializedAs("jointPrefab")] [FormerlySerializedAs("jointDisplayPrefab")] [FormerlySerializedAs("pointLightPrefab")] [FormerlySerializedAs("PointLightPrefab")] [SerializeField]
        JointSphere jointSpherePrefab = default;

        SMPLCharacter smplCharacter;
        public bool DisplayJoints => smplCharacter.DisplaySettings.displayJoints == JointDisplayState.On;

        GameObject jointContainer;

        void OnEnable() {
            smplCharacter = GetComponent<SMPLCharacter>();
            if (meshRenderer == null) meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (jointContainer == null) {
                SetupJoints();
            }
        }

        void SetupJoints() {
            jointContainer = new GameObject {name = "JointSphere Container"};
            jointContainer.transform.parent = transform;
            CreateJointsInBoneHierarchy(meshRenderer.bones[0]);
        }

        /// <summary>
        /// Walks down Bone Hierarchy to create linked point lights
        /// </summary>
        /// <param name="parent"></param>
        void CreateJointsInBoneHierarchy(Transform parent) {
            JointSphere newJointSphere = Instantiate(jointSpherePrefab, jointContainer.transform);
            newJointSphere.AttachBone(smplCharacter, this, parent);
            foreach (Transform child in parent) {
                if (Bones.IsBone(child)) {
                    CreateJointsInBoneHierarchy(child);
                }
            }
        }

        void OnDisable() {
            DestroyImmediate(jointContainer);
        }
    }
    
}
