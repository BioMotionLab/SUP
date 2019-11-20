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
    SkinnedMeshRenderer skinnedMeshRenderer;

    readonly Transform[] bones = null;

    readonly Pose[] backups;

    readonly string boneNamePrefix;

    readonly Dictionary<string, int> boneNameToJointIndex;

    //private bool _bonesAreModified = false;

    readonly Transform pelvis;

    readonly Vector3[] bonePositions;

    readonly Mesh bakedMesh = null;
    Vector3 minBounds;
    Vector3 maxBounds;

    public BMLModifyBones(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        this.skinnedMeshRenderer = skinnedMeshRenderer;
        boneNamePrefix = "";
        boneNameToJointIndex = new Dictionary<string, int>();
        boneNameToJointIndex.Add("Pelvis", 0);
        boneNameToJointIndex.Add("L_Hip", 1);
        boneNameToJointIndex.Add("R_Hip", 2);
        boneNameToJointIndex.Add("Spine1", 3);
        boneNameToJointIndex.Add("L_Knee", 4);
        boneNameToJointIndex.Add("R_Knee", 5);
        boneNameToJointIndex.Add("Spine2", 6);
        boneNameToJointIndex.Add("L_Ankle", 7);
        boneNameToJointIndex.Add("R_Ankle", 8);
        boneNameToJointIndex.Add("Spine3", 9);
        boneNameToJointIndex.Add("L_Foot", 10);
        boneNameToJointIndex.Add("R_Foot", 11);
        boneNameToJointIndex.Add("Neck", 12);
        boneNameToJointIndex.Add("L_Collar", 13);
        boneNameToJointIndex.Add("R_Collar", 14);
        boneNameToJointIndex.Add("Head", 15);
        boneNameToJointIndex.Add("L_Shoulder", 16);
        boneNameToJointIndex.Add("R_Shoulder", 17);
        boneNameToJointIndex.Add("L_Elbow", 18);
        boneNameToJointIndex.Add("R_Elbow", 19);
        boneNameToJointIndex.Add("L_Wrist", 20);
        boneNameToJointIndex.Add("R_Wrist", 21);
        boneNameToJointIndex.Add("L_Hand", 22);
        boneNameToJointIndex.Add("R_Hand", 23);
        bakedMesh = new Mesh();

        bones = skinnedMeshRenderer.bones;
        bonePositions = new Vector3[bones.Length];

        backups = new Pose[bones.Length];
        PopulateBackups();

        // Determine bone name prefix
        foreach (Transform bone in bones) {
            if (bone.name.EndsWith("root")) {
                int index = bone.name.IndexOf("root");
                boneNamePrefix = bone.name.Substring(0, index);
                break;
            }
        }
        // Determine pelvis node
        foreach (Transform bone in bones) {
            if (bone.name.EndsWith("Pelvis")) {
                pelvis = bone;
                break;
            }
        }
    }


    public void UpdateBonePositions(Vector3[] newPositions, bool feetOnGround = true)
    {
        int pelvisIndex = -1;
        for (int i = 0; i < bones.Length; i++) {
            int index;
            string boneName = bones[i].name;
            // Remove f_avg/m_avg prefix
            if (boneNamePrefix != "") {
                boneName = boneName.Replace(boneNamePrefix, "");
            }

            if (boneName == "root")
                continue;

            if (boneName == "Pelvis")
                pelvisIndex = i;

            Transform avatarTransform = skinnedMeshRenderer.transform.parent;
            if (boneNameToJointIndex.TryGetValue(boneName, out index)) {
                // Incoming new positions from joint calculation are centered at origin in world space
                // Transform to avatar position+orientation for correct world space position
                bones[i].position = avatarTransform.TransformPoint(newPositions[index]);
                bonePositions[i] = bones[i].position;
            }
            else {
                Debug.LogError("ERROR: No joint index for given bone name: " + boneName);
            }
        }

        SetBindPose(bones);
        
        if (feetOnGround) {
            RecomputeLocalBounds();
            float heightOffset = -minBounds.y;

            bones[pelvisIndex].Translate(0.0f, heightOffset, 0.0f);

            // Update bone positions to reflect new pelvis position
            for (int i = 0; i < bones.Length; i++) {
                bonePositions[i] = bones[i].position;
            }
        }
    }


    public void UpdateBoneAngles(Quaternion[] pose, Vector3 trans)  {
        int pelvisIndex = -1;

        for (int i = 0; i < bones.Length; i++) {
            string boneName = bones[i].name;

            // Remove f_avg/m_avg prefix
            if (boneNamePrefix != string.Empty) {
                boneName = boneName.Replace(boneNamePrefix, "");
                
            }

            if (boneName == "root") continue;
            if (boneName == "Pelvis") pelvisIndex = i;

            if (boneNameToJointIndex.TryGetValue(boneName, out int index)) 
            {
                bones[i].localRotation = pose[index];
            }
            else {
                Debug.LogError("ERROR: No joint index for given bone name: " + boneName);
            }
        }

        bones[pelvisIndex].localPosition = trans;
    }


    void PopulateBackups() {
        for (int i = 0; i < bones.Length; i++) {
            Vector3 p = bones[i].position;
            Quaternion r = bones[i].rotation;
            backups[i] = new Pose(p, r);
        }
    }


    // the bind pose property is actually just an array of matrices. one for each joint. The matrices are 
    // inverse transformations.
    /// <summary>
    /// Sets the bind pose of the mesh.
    /// </summary>
    /// <param name="bones"></param>
    void SetBindPose(Transform[] bones) {
        Matrix4x4[] bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;

        Transform avatarRootTransform = skinnedMeshRenderer.transform.parent;

        for (int i = 0; i < bones.Length; i++) {
            // JL: the following two comments are copied from unity documentation. 
            // The bind pose is bone's inverse transformation matrix.
            // Make this matrix relative to the avatar root so that we can move the root game object around freely.            
            // I'm pretty sure this means that the bind pose values are all in the same coordinate system. 
            // or maybe not. 
            bindPoses[i] = bones[i].worldToLocalMatrix * avatarRootTransform.localToWorldMatrix;
        }

        skinnedMeshRenderer.bones = bones;
        Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
        sharedMesh.bindposes = bindPoses;
        skinnedMeshRenderer.sharedMesh = sharedMesh;
    }


    public void ResetRotations() {
        foreach (Transform bone in bones) {
            bone.localRotation = Quaternion.identity;
        }
    }

    
    /// <summary>
    /// Finds bounding box in local space. This needs to happen manually since unity doesn't
    /// automatically recompute bounds of skinned mesh renderer after import.
    /// </summary>
    /// <param name="min">output for minimum corner</param>
    /// <param name="max">max corner</param>
    void RecomputeLocalBounds() {
        skinnedMeshRenderer.BakeMesh(bakedMesh); // I bet it's necessary to bake the mesh to 
                                             // access vertex data modified by blend shapes!
                                             // vertex coordinates are in local space. 
        Vector3[] vertices = bakedMesh.vertices;
        int numVertices = vertices.Length;

        float xMin = Mathf.Infinity;
        float xMax = Mathf.NegativeInfinity;
        float yMin = Mathf.Infinity;
        float yMax = Mathf.NegativeInfinity;
        float zMin = Mathf.Infinity;
        float zMax = Mathf.NegativeInfinity;

        for (int i = 0; i < numVertices; i++) {

            Vector3 vertex = vertices[i];
            xMin = Mathf.Min(xMin, vertex.x);
            xMax = Mathf.Max(xMax, vertex.x);
            
            yMin = Mathf.Min(yMin, vertex.y);
            yMax = Mathf.Max(yMax, vertex.y);
            
            zMin = Mathf.Min(zMin, vertex.z);
            zMax = Mathf.Max(zMax, vertex.z);
        }
        minBounds.x = xMin;
        minBounds.y = yMin;
        minBounds.z = zMin;
        
        maxBounds.x = xMax;
        maxBounds.y = yMax;
        maxBounds.z = zMax;
    }


}
