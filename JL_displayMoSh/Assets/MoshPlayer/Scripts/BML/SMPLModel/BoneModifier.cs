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
        
        readonly JointCalculator jointCalculator;
        readonly Transform pelvis;
        Vector3            minBounds;
        readonly Transform moshCharacterTransform;
        readonly SMPLSettings settings;

        public BoneModifier(ModelDefinition model, SkinnedMeshRenderer skinnedMeshRenderer, Gender gender, float[] bodyShapeBetas, SMPLSettings settings)
        {
            switch (gender) {
                case Gender.Male:
                    jointCalculator = model.MaleJointCalculator;
                    break;
                case Gender.Female:
                    jointCalculator = model.FemaleJointCalculator;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
            
            moshCharacterTransform = skinnedMeshRenderer.transform.parent;
            this.skinnedMeshRenderer = skinnedMeshRenderer;
            this.settings = settings;
            bones = skinnedMeshRenderer.bones;

            SetupBones(bodyShapeBetas);
        }


        /// <summary>
        /// Initial Setup of bones based on body shape
        /// </summary>
        /// <param name="bodyShapeBetas"></param>
        void SetupBones(float[] bodyShapeBetas) {
            Vector3[] jointPositions = jointCalculator.CalculateJointPositions(bodyShapeBetas);
            SetupBonePositions(jointPositions);
            SetupBindPoses();
            if (settings.SnapMeshFeetToGround) {
                SetFeetOnGround();
            }
        }

        /// <summary>
        /// Adjust the length and position of bones for each person
        /// </summary>
        /// <param name="jointPositions"></param>
        void SetupBonePositions(Vector3[] jointPositions) {
            for (int boneIndex = FirstBoneIndexAfterRoot; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                int boneJointIndex = Bones.NameToJointIndex[boneName];

                Vector3 newBonePositionInWorldSpace = jointPositions[boneJointIndex];
                var transformedPositionInLocalSpace = WorldPositionToCharacterSpace(newBonePositionInWorldSpace);
                bones[boneIndex].position = transformedPositionInLocalSpace;
            }
        }

        /// <summary>
        /// Incoming new positions from joint calculation are centered at origin in world space
        /// Transform to avatar position+orientation for correct world space position
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        Vector3 WorldPositionToCharacterSpace(Vector3 worldPosition) {
            Vector3 transformedPoint = moshCharacterTransform.TransformPoint(worldPosition);
            return transformedPoint;
        }

        /// <summary>
        /// The bind pose property is actually just an array of matrices. one for each joint. The matrices are inverse transformations.
        /// Sets the bind poses of the mesh.
        /// JL: the following two comments are copied from unity documentation. The bind pose is bone's inverse transformation matrix.
        /// Make this matrix relative to the avatar root so that we can move the root game object around freely.
        /// I'm pretty sure this means that the bind pose values are all in the same coordinate system. or maybe not. 
        /// </summary>
        void SetupBindPoses() {
            Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
            Matrix4x4[] newBindPoses = sharedMesh.bindposes;
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++) {
                newBindPoses[boneIndex] = bones[boneIndex].worldToLocalMatrix * moshCharacterTransform.localToWorldMatrix;
            }
            sharedMesh.bindposes = newBindPoses;
            skinnedMeshRenderer.sharedMesh = sharedMesh;
        }

        /// <summary>
        /// Updates the bones based on new poses and translation of pelvis
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="trans"></param>
        public void UpdateBones(Quaternion[] pose, Vector3 trans)  {
            for (int boneIndex = FirstBoneIndexAfterRoot; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                int poseIndex = Bones.NameToJointIndex[boneName];
                bones[boneIndex].localRotation = pose[poseIndex];
            }
            bones[PelvisIndex].localPosition = trans;
        }


        /// <summary>
        /// AB: Snaps the MESH to the ground, not the animation.
        /// This means that the lowest vertex of the character BEFORE animating is snapped to ground.
        /// I've double checked that this works properly, but several of the sample animations have parts that clip below ground.
        /// This is a problem with the MoSH process rather than the display process.
        /// The MoSH animations weren't properly calibrated or post-processed, so it gets the wrong world coordinates.
        /// </summary>
        void SetFeetOnGround() {
            RecomputeLocalBounds();
            float heightOffset = minBounds.y;
            bones[PelvisIndex].Translate(0.0f, heightOffset, 0.0f);
        }

        /// <summary>
        /// Finds bounding box in local space. Vertex coordinates are in local space.
        /// This needs to happen manually since unity doesn't automatically recompute bounds of skinned mesh renderer after import.
        /// JL: I bet it's necessary to bake the mesh to access vertex data modified by blend shapes.
        /// AB: Yes, this is the case.
        /// </summary>
        void RecomputeLocalBounds() {
            Mesh newMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(newMesh); 
            
            float xMin = Mathf.Infinity;
            float yMin = Mathf.Infinity;
            float zMin = Mathf.Infinity;
            
            float xMax = Mathf.NegativeInfinity;
            float yMax = Mathf.NegativeInfinity;
            float zMax = Mathf.NegativeInfinity;

            foreach (Vector3 vertex in newMesh.vertices) {
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
