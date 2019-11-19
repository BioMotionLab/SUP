using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VertexEntry {
    public string label;
    public int index;

    // could set up validation here. 
    public VertexEntry(string label, int index) {
        this.label = label;
        this.index = index;
    }

}
