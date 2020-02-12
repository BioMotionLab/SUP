using System.Collections.Generic;
using MoshPlayer.Scripts.BML.Display;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    [CreateAssetMenu]
    public class SettingsMain : ScriptableObject {
        
        [SerializeField]
        public List<ModelDefinition> ModelParameters = new List<ModelDefinition>();
    }
}