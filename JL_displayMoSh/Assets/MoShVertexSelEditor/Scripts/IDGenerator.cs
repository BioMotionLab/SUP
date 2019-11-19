// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26



using UnityEngine;

namespace MoShVertexSelectionBuilder {
    //[ExecuteInEditMode]
    [System.Serializable]
    [CreateAssetMenu]
    public class IDGenerator : ScriptableObject {

        [SerializeField]
        int currentID = 0;

        public int NextID {
            get {
                currentID++;
                return currentID;
            }
        }

        //	public void Awake() {
        ////		Debug.Log ("ID generator woke up.");
        //	}

        public void ResetIDs()
        {
            currentID = 0;
        }
    }
}
