using System.Collections.Generic;
using UnityEngine;

namespace MoshPlayer.Scripts.SMPLModel {
    [CreateAssetMenu]
    public class SettingsMain : ScriptableObject {
        
        [SerializeField]
        public List<ModelDefinition> ModelParameters = new List<ModelDefinition>();
    }
}