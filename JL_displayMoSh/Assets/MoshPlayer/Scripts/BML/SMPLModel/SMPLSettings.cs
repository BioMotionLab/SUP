using System;
using System.Collections.Generic;
using MoshPlayer.Scripts.BML.Display;
using MoshPlayer.Scripts.BML.FileLoaders;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    [CreateAssetMenu]
    public class SMPLSettings : ScriptableObject {
        
        [SerializeField]
        [Range(0,5)]
        public float DisplaySpeed = 1f;

        [SerializeField]
        public bool PlayBackwards = false;
        
        [SerializeField]
        public SMPLDisplaySettings DisplaySettings = default;
        
        [SerializeField]
        public bool SnapMeshFeetToGround = true;
        
        
        [SerializeField]
        public List<ModelDefinition> ModelParameters = new List<ModelDefinition>();

        public int FallbackFPS = 120;
        
    }
}