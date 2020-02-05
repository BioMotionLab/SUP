using System.Collections.Generic;
using MoshPlayer.Scripts.BML.Display;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    [CreateAssetMenu]
    public class SettingsMain : ScriptableObject {
        
        [SerializeField]
        [Range(0,5)]
        public float DisplaySpeed = 1f;

        [SerializeField]
        public bool PlayBackwards = false;
        
        [SerializeField]
        public DisplaySettings DisplaySettings = default;
        
        [SerializeField]
        public bool SnapMeshFeetToGround = true;
        
        
        [SerializeField]
        public List<ModelDefinition> ModelParameters = new List<ModelDefinition>();

        public int FallbackFPS = 120;
        
    }
}