using SMPLModel;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Settings {
    [CreateAssetMenu(fileName = "New BodyOptions Asset", menuName = Menu.AssetMenu + "New BodyOptions Asset")]
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