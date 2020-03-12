using System;
using MoshPlayer.Scripts.Display;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.SMPLModel {

    [Serializable]
    public class CharacterDisplayOptions {
        public BoneDisplayState DisplayBones;
        public PointLightDisplayState DisplayPointLights;
        public MeshDisplayState MeshDisplayState;
        public PointLightDisplayOptions PointLightDisplayOptions;
        public BoneDisplayOptions BoneDisplayOptions;
        public MeshDisplayOptions MeshDisplayOptions;
    }
    
    [Serializable]
    public class CharacterRenderOptions {
        public bool UpdateBodyShapeLive;
        public bool UpdatePosesLive;
        [FormerlySerializedAs("UpdateBlendshapesLive")]
        public bool UpdatePoseBlendshapesLive;
        public bool AllowPoseManipulation;
        [FormerlySerializedAs("UpdateTranslationLive")]
        public bool UpdateTranslationLiveY;
        public bool UpdateTranslationLiveXZ;
        [FormerlySerializedAs("SnapToground")]
        public bool SnapToGroundFirstFrame;
    }

    [Serializable]
    public class PointLightDisplayOptions {
        public float PointLightDisplaySize = 0.02f;
        public bool DrawSidesDifferentColors;
        public Material NoSideMaterial;
        public Material LeftSideMaterial;
        public Material RightSideMaterial;
    }

    [Serializable]
    public class BoneDisplayOptions {
        public float BoneWidth = 0.04f;
        public bool DrawSidesDifferentColors = default;
        public Material LeftSideMaterial = default;
        public Material RightSideMaterial = default;
    }

    [Serializable]
    public class MeshDisplayOptions {
        public Material Opaque;
        public Material SemiTransparent;

    }
    
    
}