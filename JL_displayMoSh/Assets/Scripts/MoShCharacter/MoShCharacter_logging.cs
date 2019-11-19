using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using SimpleJSON;


public partial class MoShCharacter : MonoBehaviour {

    // logging stuff.

    [HideInInspector]
    public bool LoggingJoints;

    [HideInInspector]
    public bool LoggingVerts;

    [HideInInspector]
    public VertexEntry[] vertsToLog;

    StreamWriter JointOutput;
    StreamWriter VertexOutput;
    [HideInInspector]
    public string VertLogPath;

    [HideInInspector]
    public string JointLogPath;

    [HideInInspector]
    public string SelectionFilePath;

    [HideInInspector]
    public Transform JointsRelativeTo;
    bool useRelativeCoords_j;

    [HideInInspector]
    public Transform VerticesRelativeTo;
    bool useRelativeCoords_v;


    // I'm using this to test something!
    // want the coordinates relative in terms of position, but not rotation. 
    // so move the goofball to whatever the world position is, but don't rotate it. 
    // THen make the joints relative to the goofball instead. 
    //public Transform goofball; // I don't know why I called this goofball. 
    // Soldiers in WW2 called the barbiturates they were given "goofballs". 
    // Theres your daily inane, disturbing historical fact. 
    //public bool relativePositionOnly;

    Mesh bakeTarget;

    private Vector3[] vertices;
    //List<Vector3> vertices;


    /// <summary>
    /// Only called once at the start, in order to set up the basic requirements for logging 
    /// joints and/or vertices.
    /// </summary>
    public void InitializeLogging() {
        if (LoggingVerts) {
            readVertexSelection(SelectionFilePath);
            //vertices = new List<Vector3>(meshRenderer.sharedMesh.vertexCount);
            if (VerticesRelativeTo != null) {
                useRelativeCoords_v = true;
            }
        }
        if (LoggingJoints) {
            if (JointsRelativeTo != null) {
                useRelativeCoords_j = true;
            }
        }
    }


    private void readVertexSelection(string fp) 
    {
        string jsonstring = File.ReadAllText(fp);
        JSONNode node = JSON.Parse(jsonstring);
        vertsToLog = new VertexEntry[node.Count];
        int i = 0;
        foreach (var entry in node) {
            vertsToLog[i] = new VertexEntry(entry.Key, entry.Value.AsInt);
        }
    }


    public void StartLogs() {
        if (LoggingVerts) {
            string fn = animFilename + "_verts.csv";
            string fp = Path.Combine(VertLogPath, fn);
            VertexOutput = new StreamWriter(fp);
            LogVertexHeader();
        }

        if (LoggingJoints) {
            string fn = animFilename + "_joints.csv";
            string fp = Path.Combine(JointLogPath, fn);
            JointOutput = new StreamWriter(fp);
            LogJointHeader();
        }
    }

    public void EndLogs() {
        if (LoggingJoints) {
            JointOutput.Close();
        }
        if (LoggingVerts) {
            VertexOutput.Close();
        }
    }


    public void LogJointHeader() {
        Transform[] bones = boneModifier.getBones();
        JointOutput.Write("frame");
        foreach (var joint in bones) {
            JointOutput.Write("," + joint.name + "_x");
            JointOutput.Write("," + joint.name + "_y");
            JointOutput.Write("," + joint.name + "_z");
        }
        JointOutput.WriteLine();
    }


    public void LogVertexHeader()
    {
        if (VertexOutput == null) {
            throw new Exception("Log header called before Start Log");
        }
        //StringBuilder b = new StringBuilder();
        VertexOutput.Write("frame");
        for (int i = 0; i < vertsToLog.Length; i++) {
            VertexOutput.Write("," + vertsToLog[i].label + "_x");
            VertexOutput.Write("," + vertsToLog[i].label + "_y");
            VertexOutput.Write("," + vertsToLog[i].label + "_z");
        }
        VertexOutput.WriteLine();
    }


    public void LogLine() {
        if (LoggingVerts) {
            meshRenderer.BakeMesh(bakeTarget);
            //Mesh mb = boneModifier.Baked;
            //mb.GetVertices(vertices);// yikes! Take this out soon!
            vertices = bakeTarget.vertices;
            // write the frame number. 
            VertexOutput.Write(moshFrame);
            foreach (var v in vertsToLog) {
                // p is in local space ....I think.
                Vector3 p = vertices[v.index];
                if (useRelativeCoords_v) {
                    Vector3 world = transform.TransformPoint(p);
                    p = VerticesRelativeTo.InverseTransformPoint(world);
                    VertexOutput.Write(String.Format(",{0}, {1}, {2}", p.x, p.y, p.z));
                } else {
                    // log vertices in world space.
                    Vector3 world = transform.TransformPoint(p);
                    VertexOutput.Write(String.Format(",{0}, {1}, {2}", world.x, world.y, world.z));
                }
            }
            VertexOutput.WriteLine();
        }


        if (LoggingJoints) {
            Transform[] bones = boneModifier.getBones();
            JointOutput.Write(moshFrame);
            foreach (var joint in bones) {
                Vector3 t = joint.position; // world space. 
                // use relative coordinates. 
                if (useRelativeCoords_j) {
                    //if (relativePositionOnly) {
                    //    goofball.position = JointsRelativeTo.position;
                    //    t = goofball.InverseTransformPoint(t);
                    //}
                    //else {
                    t = JointsRelativeTo.InverseTransformPoint(t);
                    //}
                }
                JointOutput.Write(String.Format(",{0}, {1}, {2}", t.x, t.y, t.z));
            }
            JointOutput.WriteLine();
        }
    }

}
