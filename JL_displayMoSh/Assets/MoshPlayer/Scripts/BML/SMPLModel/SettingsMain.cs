using System.Collections.Generic;
using MoshPlayer.Scripts.BML.Display;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    [CreateAssetMenu]
    public class SettingsMain : ScriptableObject {
        
        
        
        [SerializeField]
        public DisplaySettings DisplaySettings = default;
        
        [SerializeField]
        public List<ModelDefinition> ModelParameters = new List<ModelDefinition>();

        [SerializeField]
        public int FallbackFPS = 120;

        [SerializeField]
        public bool SetFeetOnGround = default;
    }
}