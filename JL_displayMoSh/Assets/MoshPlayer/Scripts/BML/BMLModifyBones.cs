using System.Collections.Generic;
using UnityEngine;

namespace MoshPlayer.Scripts.BML {
    /// <summary>
    /// Slightly modified version of SMPLModifyBones from MPI.
    /// Primarily just adds a reset method. 
    /// 
    /// I'm pretty sure we don't need a bones backup method, because the bone positions should
    /// be calculable using the regressors with all betas = 0. 
    /// </summary>
    public class BMLModifyBones {
    
    
        readonly SkinnedMeshRenderer     skinnedMeshRenderer;
        readonly Transform[]             bones;
        readonly Pose[]                  backups;
        readonly Dictionary<string, int> boneNameToJointIndex;
        
        readonly Transform pelvis;
        readonly Vector3[] bonePositions;
        readonly Mesh      bakedMesh = null;
        Vector3            minBounds;
        Vector3            maxBounds;
        Transform MoshCharacterTransform;

        public BMLModifyBones(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            this.skinnedMeshRenderer = skinnedMeshRenderer;
            bones = skinnedMeshRenderer.bones;
            
            boneNameToJointIndex = new Dictionary<string, int> {
                                                                   {"Pelvis", 0},
                                                                   {"L_Hip", 1},
                                                                   {"R_Hip", 2},
                                                                   {"Spine1", 3},
                                                                   {"L_Knee", 4},
                                                                   {"R_Knee", 5},
                                                                   {"Spine2", 6},
                                                                   {"L_Ankle", 7},
                                                                   {"R_Ankle", 8},
                                                                   {"Spine3", 9},
                                                                   {"L_Foot", 10},
                                                                   {"R_Foot", 11},
                                                                   {"Neck", 12},
                                                                   {"L_Collar", 13},
                                                                   {"R_Collar", 14},
                                                                   {"Head", 15},
                                                                   {"L_Shoulder", 16},
                                                                   {"R_Shoulder", 17},
                                                                   {"L_Elbow", 18},
                                                                   {"R_Elbow", 19},
                                                                   {"L_Wrist", 20},
                                                                   {"R_Wrist", 21},
                                                                   {"L_Hand", 22},
                                                                   {"R_Hand", 23}
                                                               };
            bakedMesh = new Mesh();

            
            bonePositions = new Vector3[bones.Length];

            backups = new Pose[bones.Length];
            PopulateBackups();

          
            // Determine pelvis node
            foreach (Transform bone in bones) {
                if (bone.name == "Pelvis") {
                    pelvis = bone;
                    break;
                }
            }
        }


        public void UpdateBonePositions(Vector3[] newPositions, bool feetOnGround = true) {

            int pelvisIndex = -1;
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++) {

                string boneName = bones[boneIndex].name;
                if (boneName == "root") continue;
                if (boneName == "Pelvis") pelvisIndex = boneIndex;
                
                int boneJointIndex = boneNameToJointIndex[boneName];
                
                Vector3 newBonePositionInWorldSpace = newPositions[boneJointIndex];
                var transformedPositionInLocalSpace = WorldPositionToMoshCharactersSpace(newBonePositionInWorldSpace);
                bones[boneIndex].position = transformedPositionInLocalSpace;
                bonePositions[boneIndex] = bones[boneIndex].position;

            }
            
            SetBindPoses();
        
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

        /// <summary>
        /// Incoming new positions from joint calculation are centered at origin in world space
        /// Transform to avatar position+orientation for correct world space position
        /// </summary>
        /// <param name="newBonePosition"></param>
        /// <returns></returns>
        Vector3 WorldPositionToMoshCharactersSpace(Vector3 newBonePosition) {
            
            MoshCharacterTransform = skinnedMeshRenderer.transform.parent;
            Vector3 transformedPoint = MoshCharacterTransform.TransformPoint(newBonePosition);
            return transformedPoint;
        }

       

        public void UpdateBoneAngles(Quaternion[] pose, Vector3 trans)  {
            int pelvisIndex = -1;

            for (int i = 0; i < bones.Length; i++) {
                string boneName = bones[i].name;

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

        
        /// <summary>
        /// the bind pose property is actually just an array of matrices. one for each joint. The matrices are inverse transformations.
        /// Sets the bind poses of the mesh.
        /// JL: the following two comments are copied from unity documentation. The bind pose is bone's inverse transformation matrix.
        /// Make this matrix relative to the avatar root so that we can move the root game object around freely.
        /// I'm pretty sure this means that the bind pose values are all in the same coordinate system. or maybe not. 
        /// </summary>
        void SetBindPoses() {
            Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
            Matrix4x4[] newBindPoses = sharedMesh.bindposes;
            for (int i = 0; i < bones.Length; i++) {
                newBindPoses[i] = bones[i].worldToLocalMatrix * MoshCharacterTransform.localToWorldMatrix;
            }
            sharedMesh.bindposes = newBindPoses;
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
        /// JL: I bet it's necessary to bake the mesh to access vertex data modified by blend shapes.
        /// </summary>
        void RecomputeLocalBounds() {
            skinnedMeshRenderer.BakeMesh(bakedMesh); 
            
            // vertex coordinates are in local space. 
            Vector3[] vertices = bakedMesh.vertices;
            int numVertices = vertices.Length;

            float xMin = Mathf.Infinity;
            float xMax = Mathf.NegativeInfinity;
            float yMin = Mathf.Infinity;
            float yMax = Mathf.NegativeInfinity;
            float zMin = Mathf.Infinity;
            float zMax = Mathf.NegativeInfinity;

            foreach (Vector3 vertex in bakedMesh.vertices) {
                xMin = Mathf.Min(xMin, vertex.x);
                yMin = Mathf.Min(yMin, vertex.y);
                zMin = Mathf.Min(zMin, vertex.z);
                
                xMax = Mathf.Max(xMax, vertex.x);
                yMax = Mathf.Max(yMax, vertex.y);
                zMax = Mathf.Max(zMax, vertex.z);
            }
            minBounds = new Vector3(xMin, yMin, zMin);
            maxBounds = new Vector3(xMax, yMax, zMax);
        }


    }
}
