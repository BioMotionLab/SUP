using System;
using System.Collections.Generic;
using MoshPlayer.Scripts.Display;
using UnityEngine;
using UnityEngine.Serialization;
// ReSharper disable InconsistentNaming

namespace MoshPlayer.Scripts.SMPLModel {

    [CreateAssetMenu(fileName = "Character Settings", menuName = "new Character Settings", order = 0)]
    public class CharacterSettings : ScriptableObject {
        public BoneDisplayState DisplayBones;
        public PointLightDisplayState DisplayPointLights;
        public MeshDisplayState MeshDisplayState;
        [FormerlySerializedAs("PointLightDisplayOptions")] public PointLightDisplaySettings PointLightDisplaySettings;
        public BoneDisplaySettings BoneDisplayOptions;
        public MeshDisplaySettings MeshDisplayOptions;
    }
    
    
}