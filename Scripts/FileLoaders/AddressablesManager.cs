using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace FileLoaders {
    public class AddressablesManager : MonoBehaviour {

        
        void Start() {
            AssetLabelReference key = new AssetLabelReference() {
                labelString = "Samples"
            };
            Addressables.LoadAssetsAsync<TextAsset>(key, null).Completed += LoadedSamples;
        }
        
        void LoadedSamples(AsyncOperationHandle<IList<TextAsset>> obj) {
            Debug.Log(obj.Result);
            IList<TextAsset> samples = obj.Result;
            foreach (TextAsset sample in samples) {
                Debug.Log(sample.name);
            }
        }

   

   

    }
    
    
}
