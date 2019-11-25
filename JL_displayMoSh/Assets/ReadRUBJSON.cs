using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public class ReadRUBJSON : MonoBehaviour {


    public TextAsset jsontext;

    JSONNode n;

    int frame = 0;
    int length;

    //public int headstart;

    Matrix4x4 M;

    //public Transform LeftKneeCounterpart;
    public GameObject counterpart; // MoshCharacter of counterpart. 

    Dictionary<string, string> nameCounterparts;
    Dictionary<string, Transform> dcjoints; // dictionary counter joints... in washington dc. 

    Transform[] counterpartJoints;

    float sumOfAvg = 0f;

    //const int headstart = rubLength - MoShLength;
	void Start () {

        nameCounterparts = new Dictionary<string, string>();
        nameCounterparts["HEAD"] = "Head";
        //nameCounterparts["THORAX"] = null;
        nameCounterparts["LSJC"] = "L_Shoulder";
        nameCounterparts["LEJC"] = "L_Elbow";
        nameCounterparts["LWJC"] = "L_Wrist";

        nameCounterparts["RSJC"] = "R_Shoulder";
        nameCounterparts["REJC"] = "R_Elbow";
        nameCounterparts["RWJC"] = "R_Wrist";

        nameCounterparts["PELVIS"] = "Pelvis";

        nameCounterparts["LHJC"] = "L_Hip";
        nameCounterparts["LKJC"] = "L_Knee";
        nameCounterparts["LAJC"] = "L_Ankle";

        nameCounterparts["RHJC"] = "R_Hip";
        nameCounterparts["RKJC"] = "R_Knee";
        nameCounterparts["RAJC"] = "R_Ankle";


        //counterpartChildren = counterpart;
        SkinnedMeshRenderer renderer = counterpart.GetComponent<SkinnedMeshRenderer>();
        counterpartJoints = renderer.bones;


        dcjoints = new Dictionary<string, Transform>();

        foreach (KeyValuePair<string, string> kvp in nameCounterparts) {
            dcjoints[kvp.Key] = Array.Find<Transform>(counterpartJoints,
                                                      (Transform obj) => obj.name == kvp.Value);
        }

        //foreach (var kvp in dcjoints) {
        //    Debug.Log(kvp.Key + ", " + kvp.Value.name);
        //}

        Vector3 s = new Vector3(1f, -1f, 1f); // mirror flip.
        Quaternion r = Quaternion.Euler(new Vector3(-90f, 0f, 180f));

        //Vector3 t = new Vector3(0, 0, -0.2f);
        float dz = -0.2139508f; // this is the average of the average z offset for each frame. 
        float dy = -0.05795521f;
        //float dz = -0.21f;
        Vector3 t = new Vector3(0, dy, dz);

        M = Matrix4x4.TRS(t, r, s);

        Debug.Log(M);

        n = JSON.Parse(jsontext.text);
        length = n["HEAD"].Count;
        //Debug.Log(transform.localToWorldMatrix);
        //Transform LKJC = transform.Find("LKJC");
        //Debug.Log(LKJC.localToWorldMatrix);


    }


    // Update is called once per frame
	void Update () {
        
        if (frame < length) {

      
            // repeat with y offset. 
            float avgdisplacement = 0;

            foreach (KeyValuePair<string, Transform> kvp in dcjoints) {

                // don't use ones with a weird offset. 
                if (kvp.Key != "LHJC" && kvp.Key != "RHJC" && kvp.Key != "PELVIS" && kvp.Key != "HEAD") {
                    float rubY = transform.Find(kvp.Key).position.y;
                    float moshY = kvp.Value.position.y;

                    avgdisplacement += (moshY - rubY);
                }
            }
            avgdisplacement = avgdisplacement / (dcjoints.Count - 4);

            // result is: -0.05795521

            sumOfAvg += avgdisplacement;

            float currentAvgOfAvg = sumOfAvg / (frame + 1); // frame is zero based. 


            foreach (Transform child in transform) {
                Vector3 t = n[child.name][frame].ReadVector3(Vector3.positiveInfinity);
                Vector4 p4 = new Vector4(t.x, t.y, t.z, 1f);
                Vector4 tp4 = M * p4;
                Vector4 dhtp4 = tp4 / tp4.w;
                //Vector3 tp3 = new Vector3(dhtp4.x, dhtp4.y, dhtp4.z);
                Vector3 tp3 = new Vector3(dhtp4.x, dhtp4.y, dhtp4.z);

                child.localPosition = tp3;
            }
            frame++;
        }
	}


}
