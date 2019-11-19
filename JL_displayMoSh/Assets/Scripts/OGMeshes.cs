using UnityEngine;
using UnityEditor;

public class OGMeshes : ScriptableObject {
    
    [SerializeField]
    private Mesh m;

    public Mesh Original_m {
        get {
            return m;
        }
    }


    [SerializeField]
    private Mesh f;

    public Mesh Original_f {
        get {
            return f;
        }
    }


    private void Initialize(Mesh m, Mesh f) {
        this.m = m;
        this.f = f;
    }

    //[MenuItem("MoSh/Create OG Meshes")]
    //public static void CreateOGMeshes()

}
