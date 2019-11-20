using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Slightly modified version of SMPLModifyBones from MPI.
/// Primarily just adds a reset method. 
/// 
/// I'm pretty sure we don't need a bones backup method, because the bone positions should
/// be calculable using the regressors with all betas = 0. 
/// </summary>
public class BMLModifyBones {
    SkinnedMeshRenderer targetRenderer;

    Transform[] _bones = null;

    Pose[] backups;

    string _boneNamePrefix;

    Dictionary<string, int> _boneNameToJointIndex;

    //private bool _bonesAreModified = false;

    Transform _pelvis;
    public Transform Pelvis { get { return _pelvis; } }

    Vector3[] _bonePositions;


    Mesh _bakedMesh = null;
    public Mesh Baked {
        get {
            return _bakedMesh;
        }
    }

    public BMLModifyBones(SkinnedMeshRenderer tr)
    {
        targetRenderer = tr;
        _boneNamePrefix = "";
        _boneNameToJointIndex = new Dictionary<string, int>();
        _boneNameToJointIndex.Add("Pelvis", 0);
        _boneNameToJointIndex.Add("L_Hip", 1);
        _boneNameToJointIndex.Add("R_Hip", 2);
        _boneNameToJointIndex.Add("Spine1", 3);
        _boneNameToJointIndex.Add("L_Knee", 4);
        _boneNameToJointIndex.Add("R_Knee", 5);
        _boneNameToJointIndex.Add("Spine2", 6);
        _boneNameToJointIndex.Add("L_Ankle", 7);
        _boneNameToJointIndex.Add("R_Ankle", 8);
        _boneNameToJointIndex.Add("Spine3", 9);
        _boneNameToJointIndex.Add("L_Foot", 10);
        _boneNameToJointIndex.Add("R_Foot", 11);
        _boneNameToJointIndex.Add("Neck", 12);
        _boneNameToJointIndex.Add("L_Collar", 13);
        _boneNameToJointIndex.Add("R_Collar", 14);
        _boneNameToJointIndex.Add("Head", 15);
        _boneNameToJointIndex.Add("L_Shoulder", 16);
        _boneNameToJointIndex.Add("R_Shoulder", 17);
        _boneNameToJointIndex.Add("L_Elbow", 18);
        _boneNameToJointIndex.Add("R_Elbow", 19);
        _boneNameToJointIndex.Add("L_Wrist", 20);
        _boneNameToJointIndex.Add("R_Wrist", 21);
        _boneNameToJointIndex.Add("L_Hand", 22);
        _boneNameToJointIndex.Add("R_Hand", 23);
        _bakedMesh = new Mesh();

        _bones = targetRenderer.bones;
        _bonePositions = new Vector3[_bones.Length];

        backups = new Pose[_bones.Length];
        populateBackups();

        // Determine bone name prefix
        foreach (Transform bone in _bones) {
            if (bone.name.EndsWith("root")) {
                int index = bone.name.IndexOf("root");
                _boneNamePrefix = bone.name.Substring(0, index);
                break;
            }
        }
        // Determine pelvis node
        foreach (Transform bone in _bones) {
            if (bone.name.EndsWith("Pelvis")) {
                _pelvis = bone;
                break;
            }
        }
    }


    public Transform[] getBones()
    {
        return _bones;
    }

    public Dictionary<string, int> getB2J_indices()
    {
        return _boneNameToJointIndex;
    }

    public Transform getPelvis()
    {
        return _pelvis;
    }

    //public Vector3[] getBonePositions()
    //{
    //    return _bonePositions;
    //}

    public string getBoneNamePrefix()
    {
        return _boneNamePrefix;
    }

    public void updateBonePositions(Vector3[] newPositions, bool feetOnGround = true)
    {
        int pelvisIndex = -1;
        for (int i = 0; i < _bones.Length; i++) {
            int index;
            string boneName = _bones[i].name;
            // Remove f_avg/m_avg prefix
            if (_boneNamePrefix != "") {
                boneName = boneName.Replace(_boneNamePrefix, "");
            }

            if (boneName == "root")
                continue;

            if (boneName == "Pelvis")
                pelvisIndex = i;

            Transform avatarTransform = targetRenderer.transform.parent;
            if (_boneNameToJointIndex.TryGetValue(boneName, out index)) {
                // Incoming new positions from joint calculation are centered at origin in world space
                // Transform to avatar position+orientation for correct world space position
                _bones[i].position = avatarTransform.TransformPoint(newPositions[index]);
                _bonePositions[i] = _bones[i].position;
            }
            else {
                Debug.LogError("ERROR: No joint index for given bone name: " + boneName);
            }
        }

        _setBindPose(_bones);
        if (feetOnGround) {
            Vector3 min = new Vector3();
            Vector3 max = new Vector3();
            _localBounds(ref min, ref max);
            float heightOffset = -min.y;

            _bones[pelvisIndex].Translate(0.0f, heightOffset, 0.0f);

            // Update bone positions to reflect new pelvis position
            for (int i = 0; i < _bones.Length; i++) {
                _bonePositions[i] = _bones[i].position;
            }
        }
    }


    public void updateBoneAngles(Quaternion[] pose, Vector3 trans)
    {
        int pelvisIndex = -1;

        for (int i = 0; i < _bones.Length; i++) {
            int index;
            string boneName = _bones[i].name;

            // Remove f_avg/m_avg prefix
            if (_boneNamePrefix != string.Empty) {
                boneName = boneName.Replace(_boneNamePrefix, "");
                
            }

            if (boneName == "root") {
                continue;
            }

            if (boneName == "Pelvis")
                pelvisIndex = i;

            if (_boneNameToJointIndex.TryGetValue(boneName, out index)) 
            {
                _bones[i].localRotation = pose[index];
            }
            else {
                Debug.LogError("ERROR: No joint index for given bone name: " + boneName);
            }
        }

        _bones[pelvisIndex].localPosition = trans;
        //_bonesAreModified = true;
    }


    void populateBackups()
    {
        for (int i = 0; i < _bones.Length; i++) {
            Vector3 p = _bones[i].position;
            Quaternion r = _bones[i].rotation;
            backups[i] = new Pose(p, r);
        }
    }


    void restoreBones2() {
        for (int i = 0; i < _bones.Length; i++) {
            _bones[i].position = backups[i].position;
            _bones[i].rotation = backups[i].rotation;
        }
    }


    // the bind pose property is actually just an array of matrices. one for each joint. The matrices are 
    // inverse transformations.
    /// <summary>
    /// Sets the bind pose of the mesh.
    /// </summary>
    /// <param name="bones"></param>
    void _setBindPose(Transform[] bones)
    {
        Matrix4x4[] bindPoses = targetRenderer.sharedMesh.bindposes;

        Transform avatarRootTransform = targetRenderer.transform.parent;

        for (int i = 0; i < bones.Length; i++) {
            // JL: the following two comments are copied from unity documentation. 
            // The bind pose is bone's inverse transformation matrix.
            // Make this matrix relative to the avatar root so that we can move the root game object around freely.            
            // I'm pretty sure this means that the bind pose values are all in the same coordinate system. 
            // or maybe not. 
            bindPoses[i] = bones[i].worldToLocalMatrix * avatarRootTransform.localToWorldMatrix;
        }

        targetRenderer.bones = bones;
        Mesh sharedMesh = targetRenderer.sharedMesh;
        sharedMesh.bindposes = bindPoses;
        targetRenderer.sharedMesh = sharedMesh;
    }


    /// <summary>
    /// Reset bones to original transform, and set the bind pose back to the original.
    /// </summary>
    public void reset()
    {
        //if (!_bonesAreModified) {
        //    return;
        //}
        if ((_bones != null)) {
            //_restoreBones();
            restoreBones2();
            _setBindPose(_bones);
            //_bonesAreModified = false;
        }
    }


    public void ResetRotations()
    {
        for (int i = 0; i < _bones.Length; i++) {
            _bones[i].localRotation = Quaternion.identity;
        }
    }

    // THIS IS WEIRD BECAUSE SHARED MESH RENDERER CAN FIND THE AABB in local space. 
    /// <summary>
    /// Finds bounding box in local space.
    /// </summary>
    /// <param name="min">output for minimum corner</param>
    /// <param name="max">max corner</param>
    void _localBounds(ref Vector3 min, ref Vector3 max)
    {
        targetRenderer.BakeMesh(_bakedMesh); // I bet it's necessary to bake the mesh to 
                                             // access vertex data modified by blend shapes!
                                             // vertex coordinates are in local space. 
        Vector3[] vertices = _bakedMesh.vertices;
        int numVertices = vertices.Length;

        float xMin = Mathf.Infinity;
        float xMax = Mathf.NegativeInfinity;
        float yMin = Mathf.Infinity;
        float yMax = Mathf.NegativeInfinity;
        float zMin = Mathf.Infinity;
        float zMax = Mathf.NegativeInfinity;

        for (int i = 0; i < numVertices; i++) {
            Vector3 v = vertices[i];
            if (v.x < xMin) {
                xMin = v.x;
            }
            else if (v.x > xMax) {
                xMax = v.x;
            }
            if (v.y < yMin) {
                yMin = v.y;
            }
            else if (v.y > yMax) {
                yMax = v.y;
            }
            if (v.z < zMin) {
                zMin = v.z;
            }
            else if (v.z > zMax) {
                zMax = v.z;
            }
        }
        min.x = xMin;
        min.y = yMin;
        min.z = zMin;
        max.x = xMax;
        max.y = yMax;
        max.z = zMax;
    }


}
