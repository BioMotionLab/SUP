using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SMPLModel {
    [CreateAssetMenu]
    public class Models : ScriptableObject {
        
        [FormerlySerializedAs("ModelDefinitions")]
        [FormerlySerializedAs("ModelParameters")]
        [SerializeField]
        public List<ModelDefinition> modelDefinitions = new List<ModelDefinition>();
    }
}