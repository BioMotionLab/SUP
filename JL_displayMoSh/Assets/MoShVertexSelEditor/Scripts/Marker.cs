// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26


using UnityEngine;

namespace MoShVertexSelectionBuilder {
    [System.Serializable]
    public class Marker {
        public string label;
        public int vertex;
        //	public bool isEdited;
        public Color displayColor;
        public int markersetIndex;
        public bool justCreated;

        public bool IsSelected = false;

        public int uniqueID;

        public Marker(string label, int vertex, int setIndex, int id)
        {
            this.label = label;
            this.vertex = vertex;
            //		this.isEdited = isEdited;
            markersetIndex = setIndex;
            displayColor = Color.black; // default color to mark in the scene view.
            justCreated = false;
            uniqueID = id;
        }

        //	public Marker(string label, int vertex, int setIndex, bool justCreated) 
        //		: this(label, vertex, setIndex) 
        //	{
        //		this.justCreated = justCreated;		
        //	}

        public Marker(string label, int vertex, int setIndex, int id, bool justCreated)
        {
            this.justCreated = justCreated;
            this.label = label;
            this.vertex = vertex;
            //		this.isEdited = isEdited;
            markersetIndex = setIndex;
            displayColor = Color.black; // default color to mark in the scene view.
                                        //		this.justCreated = ;
            uniqueID = id;
        }
    }
}

