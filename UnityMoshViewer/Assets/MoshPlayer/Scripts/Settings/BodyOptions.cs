using MoshPlayer.Scripts.SMPLModel;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace MoshPlayer {
    [CreateAssetMenu(fileName = "Body Update Options", menuName = "New Body Update Options", order = 0)]
    public class BodyOptions : ScriptableObject {
        public bool ShowIndividualizedBody;
        public bool UpdateBodyShapeLive;
        public bool UpdatePosesLive;
        public bool UpdatePoseBlendshapesLive;
        public bool AllowPoseManipulation;
        public bool UpdateTranslationLiveY;
        public bool UpdateTranslationLiveXZ;
        public GroundSnapType GroundSnap;
    }
    
}