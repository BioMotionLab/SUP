using SMPLModel;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Settings {
    [CreateAssetMenu(fileName = "New BodySettings Asset", menuName = Menu.AssetMenu + "New BodySettings Asset")]
    public class BodySettings : ScriptableObject {
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