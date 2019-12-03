using System.Collections.Generic;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    /// <summary>
    /// Slightly modified version of SMPLModifyBones from MPI.
    /// Primarily just adds a reset method. 
    /// 
    /// </summary>
    public class BoneModifier {
    
    
        const int PelvisIndex = 1;
        const int FirstBoneIndexAfterRoot = 1;

        readonly SkinnedMeshRenderer     skinnedMeshRenderer;
        readonly Transform[]             bones;
        readonly Dictionary<string, int> boneNameToJointIndex;
        
        readonly Transform pelvis;
        readonly Mesh      bakedMesh = null;
        Vector3            minBounds;
        Transform moshCharacterTransform;
        readonly SMPLSettings settings;

        public BoneModifier(SkinnedMeshRenderer skinnedMeshRenderer, SMPLSettings settings)
        {
            
            this.skinnedMeshRenderer = skinnedMeshRenderer;
            this.settings = settings;
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
        }


        public void UpdateBonePositions(Vector3[] newPositions) {
            SetBonePositions(newPositions);
            SetBindPoses();
            if (settings.CharacterFeetSnapToGround) {
                SetFeetOnGround();
            }
        }

        void SetFeetOnGround() {
            
            RecomputeLocalBounds();
            float heightOffset = -minBounds.y;
            bones[PelvisIndex].Translate(0.0f, heightOffset, 0.0f);
            
        }

        void SetBonePositions(Vector3[] newPositions) {
            for (int boneIndex = FirstBoneIndexAfterRoot; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                int boneJointIndex = boneNameToJointIndex[boneName];

                Vector3 newBonePositionInWorldSpace = newPositions[boneJointIndex];
                var transformedPositionInLocalSpace = WorldPositionToMoshCharactersSpace(newBonePositionInWorldSpace);
                bones[boneIndex].position = transformedPositionInLocalSpace;
            }
        }

        /// <summary>
        /// Incoming new positions from joint calculation are centered at origin in world space
        /// Transform to avatar position+orientation for correct world space position
        /// </summary>
        /// <param name="newBonePosition"></param>
        /// <returns></returns>
        Vector3 WorldPositionToMoshCharactersSpace(Vector3 newBonePosition) {
            moshCharacterTransform = skinnedMeshRenderer.transform.parent;
            Vector3 transformedPoint = moshCharacterTransform.TransformPoint(newBonePosition);
            return transformedPoint;
        }

        public void UpdateBoneAngles(Quaternion[] pose, Vector3 trans)  {
            for (int boneIndex = FirstBoneIndexAfterRoot; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                int poseIndex = boneNameToJointIndex[boneName];
                bones[boneIndex].localRotation = pose[poseIndex];
            }
            bones[PelvisIndex].localPosition = trans;
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
                newBindPoses[i] = bones[i].worldToLocalMatrix * moshCharacterTransform.localToWorldMatrix;
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
        /// 
        /// Vertex coordinates are in local space. 
        /// </summary>
        void RecomputeLocalBounds() {
            skinnedMeshRenderer.BakeMesh(bakedMesh); 
            
            
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
            //maxBounds = new Vector3(xMax, yMax, zMax);
        }


    }
}
