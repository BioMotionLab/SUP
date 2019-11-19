using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Control point light display on a 
/// </summary>
public class PointLightDisplay : MonoBehaviour {

    public Transform PLPrefab;

    //public Material PLshading;
    public Color PLcolor = Color.white;

    // about twice the radius of my wrist in meters. 
    public float radius = 0.05f;

    [HideInInspector]
    public bool pldInstantiated;

    public bool excludeFeet;

    [HideInInspector]
    public string plName;

    /// <summary>
    /// This behavior is mostly meant to be used in the editor. Add the component,
    /// instantiate the point lights, adjust if needed, and then start the program. 
    /// However, the functions InstantiatePointLights() and RemovePointLights() can 
    /// be used to instantiate or remove point light displays at runtime if needed.
    /// If a point light display component is active in the inspector, then. 
    /// </summary>
    public void InstantiatePointLights()
    {
        // loop through all bones adding a point light representation. 
        var m = new Material(PLPrefab.GetComponent<MeshRenderer>().sharedMaterial);
        m.color = PLcolor;
        //PLs = new List<Transform>();
        var smr = GetComponent<SkinnedMeshRenderer>();
        float scale = 2 * radius;
        foreach (var bone in smr.bones) {
            if (bone == smr.rootBone) { // don't want to add a light to root. 
                continue;
            }
            if (excludeFeet && (bone.name == "L_Foot" || bone.name == "R_Foot"))
                continue;
            
            Transform PLtrans = Instantiate(PLPrefab, bone, false);
            PLtrans.gameObject.name = "PointLight";

            PLtrans.localPosition = Vector3.zero;
            PLtrans.localScale = new Vector3(scale, scale, scale);
            PLtrans.gameObject.GetComponent<MeshRenderer>().sharedMaterial = m;
        }
        smr.enabled = false;
        pldInstantiated = true;
    }


    //public void RemovePointLights()
    //{
    //    List<Transform> L = PLs;
    //    PLs = null;
    //    for (int i = 0; i < L.Count; i++) {
    //        GameObject g = L[i].gameObject;
    //        L[i] = null;
    //        Destroy(g);
    //    }
    //    var smr = GetComponent<SkinnedMeshRenderer>();
    //    smr.enabled = true;

    //}


	void Start()
	{
        if (!pldInstantiated) {
            InstantiatePointLights();
        }
	}

}
