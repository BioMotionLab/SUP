using System;
using MoshPlayer.Scripts.Display;
using UnityEngine;
using UnityEngine.Serialization;
// ReSharper disable InconsistentNaming

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
        public bool ShowIndividualizedBody;
        public bool UpdateBodyShapeLive;
        public bool UpdatePosesLive;
        public bool UpdatePoseBlendshapesLive;
        public bool AllowPoseManipulation;
        public bool UpdateTranslationLiveY;
        public bool UpdateTranslationLiveXZ;
        public bool SnapToGroundFirstFrame;
    }

    [Serializable]
    public class PointLightDisplayOptions {
        public float PointLightDisplaySize = 0.02f;
        public bool DrawSidesDifferentColors;
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