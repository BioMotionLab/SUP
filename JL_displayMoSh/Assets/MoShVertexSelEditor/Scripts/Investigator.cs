// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26


using System.Collections.Generic;
using UnityEngine;
using System;


namespace MoShVertexSelectionBuilder {
    [ExecuteInEditMode]
    public class Investigator : MonoBehaviour {

        MeshFilter mf;

        //	Vector3[] verts
        public bool mappingBuilt = false;

        List<Vector3> verts;

        public float vertexDisplaySize = 0.01f;

        [NonSerialized]
        public Dictionary<int, int> mapping;

        void Awake()
        {
            mf = GetComponent<MeshFilter>();
            verts = new List<Vector3>();
            Mesh sm = mf.sharedMesh;
            sm.GetVertices(verts);

            Debug.Assert(sm.vertexCount == verts.Count, "vertex # measures not equal.");
            Debug.Log(sm.vertexCount);
            Debug.Assert(sm.vertexBufferCount == 1, "more than one vertex buffer.");
        }

        // Populate int, int dictionary
        public void FindDuplicates()
        {
            // I'm taking a precomputation approach. I will have to test if this works on PC as well. 
            // and with later versions of unity. Mesh stuff may have been changed with Unity 17.3, since they
            // added support for 32 bit mesh buffers, over 16 bits (current limit).
            // if the order is always the same, then it will be fine. 
            // Dictionary<int, int> mapping = new Dictionary<int, int> ();
            int duplicateCount = 0;
            mapping = new Dictionary<int, int>();
            for (int i = 6890; i < verts.Count; i++) {
                int rslt = verts.FindIndex(v => v == verts[i]);
                mapping[i] = rslt;
                Debug.Log("searching for vertex " + i + " result: " + rslt);
                Debug.Assert(rslt != -1, "No match found! (this is impossible)");
                Debug.Assert(rslt <= 6889, "match found greater than 6889!");
                Debug.Assert(rslt != i, "no duplicate vertex for vertex at index: " + i);

                if (rslt != -1 && rslt <= 6889) {
                    duplicateCount++;
                }
            }
            Debug.Assert(duplicateCount == verts.Count - 6890, "some vertices over 6889 had no duplicate");
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            for (int i = 6890; i < verts.Count; i++) {
                Gizmos.DrawSphere(transform.TransformPoint(verts[i]), vertexDisplaySize);
            }
        }
    }
}
