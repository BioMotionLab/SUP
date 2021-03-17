using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

public class AddressablesManager : MonoBehaviour {

    [FormerlySerializedAs("assetReference")] [SerializeField] AssetReference JsonAssetReference;

    [SerializeField] AssetReference h5Reference;
    // Start is called before the first frame update
    void Start() {
        
    }

    void JSONLoaded(AsyncOperationHandle<TextAsset> obj) {
        Debug.Log("loaded asset");
        Debug.Log(obj.Result.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
