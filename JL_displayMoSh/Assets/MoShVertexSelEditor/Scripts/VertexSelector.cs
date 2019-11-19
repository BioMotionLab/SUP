// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26


using System.Collections.Generic;
using UnityEngine;



namespace MoShVertexSelectionBuilder {
    /// <summary>
    /// Vertex selector. Component used to select a vertex in the editor. 
    /// May not work correctly for meshes that have submeshes.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public class VertexSelector : MonoBehaviour {

        Dictionary<int, int> duplicateMapping;

        [Range(0.01f, 0.1f)]
        public float markersize = 0.025f;


        Vector3[] verts;
        int[] tris;

        // this is a nullable type, 
        int? vertIndex = null; //nullable int.
                               /// <summary>
                               /// get the index of selected vertex or null.
                               /// </summary>
                               /// <remarks>
                               /// If you haven't seen a nullable type before (indicated with ?)
                               /// it is one of several ways to let a value type be null.
                               /// see: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/nullable-types/index
                               /// 
                               /// It seems to work exactly like Maybe in Haskell and other functional programming languages. 
                               /// 
                               /// I don't know if this has any bearing on performance or the odd details of Unity's C# 
                               /// </remarks>
        public int? VertIndex {
            get {
                return vertIndex;
            }
        }

        public bool HasSelection {
            get {
                return vertIndex.HasValue;
            }
        }


        void Awake()
        {
            //		Debug.Log ("Awake called");
            initialize();
        }

        public void initialize()
        {
            Mesh m = GetComponent<MeshFilter>().sharedMesh;
            GetComponent<MeshCollider>().sharedMesh = m;
            verts = m.vertices;
            tris = m.triangles;
        }

        public void TargetHit(RaycastHit hit)
        {

            // Vertex coords are always in local space.
            Vector3 collisionPointLocal = transform.InverseTransformPoint(hit.point);

            // get the triangle that was hit, get indices of component vertices, 
            // compare distance from collision to each vertex in local space.  
            // select lowest value.
            // Might be important to check if any of them are sufficiently close to collision point. 
            int triIndex = hit.triangleIndex;
            int vi1 = tris[triIndex * 3];
            int vi2 = tris[triIndex * 3 + 1];
            int vi3 = tris[triIndex * 3 + 2];

            float distV1 = Vector3.Distance(collisionPointLocal, verts[vi1]);
            float distV2 = Vector3.Distance(collisionPointLocal, verts[vi2]);
            float distV3 = Vector3.Distance(collisionPointLocal, verts[vi3]);

            if (distV1 < distV2 && distV1 < distV3) {
                vertIndex = vi1;
            }
            else if (distV2 < distV1 && distV2 < distV3) {
                vertIndex = vi2;
            }
            else if (distV3 < distV1 && distV3 < distV2) {
                vertIndex = vi3;
            }

        }

        public void ClearSelection()
        {
            vertIndex = null;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (vertIndex.HasValue) {
                Vector3 vert = verts[vertIndex.Value];
                Vector3 worldVert = transform.TransformPoint(vert);
                Gizmos.DrawCube(worldVert, new Vector3(markersize, markersize, markersize));
            }
        }

    }
}
