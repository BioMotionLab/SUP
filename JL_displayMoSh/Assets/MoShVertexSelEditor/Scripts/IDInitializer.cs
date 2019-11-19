// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26

using UnityEditor;


namespace MoShVertexSelectionBuilder {
    [InitializeOnLoad]
    public class IDInitializer {

        static IDInitializer()
        {
            IDGenerator ids = AssetDatabase.LoadAssetAtPath<IDGenerator>(
                "Assets/MoShVertexSelEditor/MarkerIDGenerator.asset"
            );
            ids.ResetIDs(); // initialize the thing to zero. 
                            //		Debug.Log ("IDs reset.");
        }

    }
}
