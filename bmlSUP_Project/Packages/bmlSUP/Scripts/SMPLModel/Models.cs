using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SMPLModel {
    [CreateAssetMenu]
    public class Models : ScriptableObject {
        
        [FormerlySerializedAs("ModelParameters")]
        [SerializeField]
        public List<ModelDefinition> ModelDefinitions = new List<ModelDefinition>();
    }
}