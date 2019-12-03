using MoshPlayer.Scripts.BML.SMPLModel;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.Display {
    public class BoneRenderer : MonoBehaviour {

        LineRenderer lineRenderer;

        Transform child;
        Transform parent;

        SMPLSettings settings;
    
        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void Init(Transform boneParent, Transform boneChild, SMPLSettings settingsFile) {
            parent = boneParent;
            child = boneChild;
            settings = settingsFile;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3[] positions = {child.position, parent.position};
            lineRenderer.SetPositions(positions);
            lineRenderer.startWidth = settings.DisplaySettings.BoneWidth;
            lineRenderer.endWidth = settings.DisplaySettings.BoneWidth;

            if (!settings.DisplaySettings.DrawSidesDifferentColors) return;
            
            
            if (child.name.Contains(SMPLConstants.LeftBonePrefix)) {
                lineRenderer.material = settings.DisplaySettings.LeftSideMaterial;
            }
            if (child.name.Contains(SMPLConstants.RightBonePrefix)) {
                lineRenderer.material = settings.DisplaySettings.RightSideMaterial;
            }

        }
    
    }
}
