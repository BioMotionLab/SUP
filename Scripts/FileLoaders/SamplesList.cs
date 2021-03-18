using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace FileLoaders {
    
    [CreateAssetMenu]
    public class SamplesList : ScriptableObject {
        [FormerlySerializedAs("sampleAssets")] public List<SampleGroup> SampleGroups = default;

    }

    [Serializable]
    public class SampleGroup {
        [SerializeField]
        public List<AssetReference> samples = default;
    }
}
