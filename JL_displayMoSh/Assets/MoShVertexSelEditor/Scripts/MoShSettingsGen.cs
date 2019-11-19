// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26

using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System;

namespace MoShVertexSelectionBuilder {
    [ExecuteInEditMode]
    [RequireComponent(typeof(VertexSelector))]
    public class MoShSettingsGen : MonoBehaviour {


        [HideInInspector]
        public JSONNode JsonTree;

        [HideInInspector]
        public Genders gender;

        [Range(0.01f, 0.05f)]
        public float markersize = 0.015f;

        public List<Marker> Markers;

        Vector3[] verts;

        [HideInInspector] // Nope, needs to be serialized, otherwise it loses it when it's deselected.
        public bool SettingsLoaded = false;

        //	[HideInInspector]
        //Do not want this reference to be serialized. I think it will fix a problem. 
        // actually, maybe this should be serialized.
        [NonSerialized]
        public Marker selection;

        public void MarkerSelected(int id)
        {
            if (selection != null)
                selection.IsSelected = false;
            selection = Markers.Find((Marker m) => m.uniqueID == id);
            selection.IsSelected = true;
        }


        void Awake()
        {

            Markers = new List<Marker>();
            verts = GetComponent<MeshFilter>().sharedMesh.vertices;
        }

        public void AssignMesh(Mesh m)
        {
            GetComponent<MeshFilter>().sharedMesh = m;
            GetComponent<MeshCollider>().sharedMesh = m;

            verts = m.vertices;
            foreach (var marker in Markers) {
                // not exactly an ideal solution, but I don't want any array out of bounds errors.
                if (marker.vertex > verts.Length) {
                    marker.vertex = -999;
                }
            }
        }

        public void DeleteSelected()
        {
            if (selection != null) {
                Markers.Remove(selection);
                selection = null;
            }
        }



        void OnDrawGizmos()
        {
            Vector3 drawposition;
            int mvertex;
            foreach (var marker in Markers) {
                mvertex = marker.vertex;
                if (mvertex >= 0) {
                    drawposition = transform.TransformPoint(verts[mvertex]);
                    //				Gizmos.color = marker.displayColor;
                    if (marker.IsSelected)
                        Gizmos.color = Color.magenta;
                    else
                        Gizmos.color = Color.black;
                    Gizmos.DrawSphere(drawposition, markersize);
                }
            }
        }
    }
}
