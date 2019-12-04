using System;
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
        readonly JointCalculator jointCalculator;
        readonly Transform pelvis;
        readonly Mesh      bakedMesh = null;
        Vector3            minBounds;
        Transform moshCharacterTransform;
        readonly SMPLSettings settings;

        public BoneModifier(SkinnedMeshRenderer skinnedMeshRenderer, Gender gender, float[] betas, SMPLSettings settings)
        {
            switch (gender) {
                case Gender.Male:
                    jointCalculator = settings.MaleJointCalculator;
                    break;
                case Gender.Female:
                    jointCalculator = settings.FemaleJointCalculator;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
            
            moshCharacterTransform = skinnedMeshRenderer.transform.parent;
            this.skinnedMeshRenderer = skinnedMeshRenderer;
            this.settings = settings;
            bones = skinnedMeshRenderer.bones;
            
            boneNameToJointIndex = new Dictionary<string, int> {
                                                                   {Bones.Pelvis, 0},
                                                                   {Bones.LeftHip, 1},
                                                                   {Bones.RightHip, 2},
                                                                   {Bones.Spine1, 3},
                                                                   {Bones.LeftKnee, 4},
                                                                   {Bones.RightKnee, 5},
                                                                   {Bones.Spine2, 6},
                                                                   {Bones.LeftAnkle, 7},
                                                                   {Bones.RightAnkle, 8},
                                                                   {Bones.Spine3, 9},
                                                                   {Bones.LeftFoot, 10},
                                                                   {Bones.RightFoot, 11},
                                                                   {Bones.Neck, 12},
                                                                   {Bones.LeftCollar, 13},
                                                                   {Bones.RightCollar, 14},
                                                                   {Bones.Head, 15},
                                                                   {Bones.LeftShoulder, 16},
                                                                   {Bones.RightShoulder, 17},
                                                                   {Bones.LeftElbow, 18},
                                                                   {Bones.RightElbow, 19},
                                                                   {Bones.LeftWrist, 20},
                                                                   {Bones.RightWrist, 21},
                                                                   {Bones.LeftHand, 22},
                                                                   {Bones.RightHand, 23}
                                                               };
            bakedMesh = new Mesh();
            
            SetupBones(betas);
            
        }


        /// <summary>
        /// Gets the new joint positions from the animation.
        /// Passes them to the boneModifier. 
        /// </summary>
        /// <param name="betas"></param>
        void SetupBones(float[] betas) {
            Vector3[] jointPositions = jointCalculator.CalculateJointPositions(betas);
            SetupBonePositions(jointPositions);
            SetupBindPoses();
            if (settings.CharacterFeetSnapToGround) {
                SetFeetOnGround();
            }
        }

        void SetupBonePositions(Vector3[] jointPositions) {
            for (int boneIndex = FirstBoneIndexAfterRoot; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                int boneJointIndex = boneNameToJointIndex[boneName];

                Vector3 newBonePositionInWorldSpace = jointPositions[boneJointIndex];
                var transformedPositionInLocalSpace = WorldPositionToMoshCharactersSpace(newBonePositionInWorldSpace);
                bones[boneIndex].position = transformedPositionInLocalSpace;
            }
        }

        /// <summary>
        /// Incoming new positions from joint calculation are centered at origin in world space
        /// Transform to avatar position+orientation for correct world space position
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        Vector3 WorldPositionToMoshCharactersSpace(Vector3 worldPosition) {
            Vector3 transformedPoint = moshCharacterTransform.TransformPoint(worldPosition);
            return transformedPoint;
        }

        /// <summary>
        /// the bind pose property is actually just an array of matrices. one for each joint. The matrices are inverse transformations.
        /// Sets the bind poses of the mesh.
        /// JL: the following two comments are copied from unity documentation. The bind pose is bone's inverse transformation matrix.
        /// Make this matrix relative to the avatar root so that we can move the root game object around freely.
        /// I'm pretty sure this means that the bind pose values are all in the same coordinate system. or maybe not. 
        /// </summary>
        void SetupBindPoses() {
            Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
            Matrix4x4[] newBindPoses = sharedMesh.bindposes;
            for (int i = 0; i < bones.Length; i++) {
                newBindPoses[i] = bones[i].worldToLocalMatrix * moshCharacterTransform.localToWorldMatrix;
            }
            sharedMesh.bindposes = newBindPoses;
            skinnedMeshRenderer.sharedMesh = sharedMesh;
        }

        /// <summary>
        /// Update the bone movements based on new poses and translations
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="trans"></param>
        public void UpdateBoneRotations(Quaternion[] pose, Vector3 trans)  {
            for (int boneIndex = FirstBoneIndexAfterRoot; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                int poseIndex = boneNameToJointIndex[boneName];
                bones[boneIndex].localRotation = pose[poseIndex];
            }
            bones[PelvisIndex].localPosition = trans;
        }


        void SetFeetOnGround() {
            RecomputeLocalBounds();
            float heightOffset = minBounds.y;
            bones[PelvisIndex].Translate(0.0f, heightOffset, 0.0f);
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
            float yMin = Mathf.Infinity;
            float zMin = Mathf.Infinity;
            
            float xMax = Mathf.NegativeInfinity;
            float yMax = Mathf.NegativeInfinity;
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
