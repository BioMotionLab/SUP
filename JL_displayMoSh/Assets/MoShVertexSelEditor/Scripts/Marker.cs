// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26


using UnityEngine;

namespace MoShVertexSelectionBuilder {
    [System.Serializable]
    public class Marker {
        public string label;
        public int vertex;
        public Color displayColor;
        public int markersetIndex;
        public bool justCreated;

        public bool IsSelected = false;

        public int uniqueID;

        public Marker(string label, int vertex, int setIndex, int id)
        {
            this.label = label;
            this.vertex = vertex;
            markersetIndex = setIndex;
            displayColor = Color.black; // default color to mark in the scene view.
            justCreated = false;
            uniqueID = id;
        }


        public Marker(string label, int vertex, int setIndex, int id, bool justCreated)
        {
            this.justCreated = justCreated;
            this.label = label;
            this.vertex = vertex;
            markersetIndex = setIndex;
            displayColor = Color.black; // default color to mark in the scene view.
            uniqueID = id;
        }
    }
}

