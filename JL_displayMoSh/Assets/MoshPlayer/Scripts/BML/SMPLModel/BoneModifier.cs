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
        
        const int FirstBoneIndexAfterRoot = 1;

        readonly SkinnedMeshRenderer     skinnedMeshRenderer;
        readonly Transform[]             bones;
        
        readonly Transform pelvis;
        Vector3            minBounds;
        readonly Transform moshCharacterTransform;
        readonly SMPLSettings settings;
        private ModelDefinition model;

        public BoneModifier(ModelDefinition model, SkinnedMeshRenderer skinnedMeshRenderer, Gender gender, float[] bodyShapeBetas, SMPLSettings settings)
        {
            this.model = model;
            moshCharacterTransform = skinnedMeshRenderer.transform.parent;
            this.skinnedMeshRenderer = skinnedMeshRenderer;
            this.settings = settings;
            bones = skinnedMeshRenderer.bones;

            if (settings.SnapMeshFeetToGround) {
                SetFeetOnGround();
            }
        }
        
        /// <summary>
        /// Updates the bones based on new poses and translation of pelvis
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="trans"></param>
        public void UpdateBones(Quaternion[] pose, Vector3 trans)  {
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                if (boneName == Bones.Pelvis) continue;
                try
                {
                    int poseIndex = Bones.NameToJointIndex[boneName];
                    bones[boneIndex].localRotation = pose[poseIndex];
                }
                catch (KeyNotFoundException e)
                {
                    throw new KeyNotFoundException($"Bone Not in dictionary: boneIndex: {boneIndex}, name: {boneName}");
                }
                
            }
            bones[model.PelvisIndex].localPosition = trans;
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
            if (Mathf.Abs(heightOffset) > 500)
            {
                Debug.LogError("heightOffset calculated incorrectly");
                return;
            }

            Transform pelvisBone = bones[model.PelvisIndex];
            Debug.Log(pelvisBone.name);
            Debug.Log(pelvisBone.position);
            Debug.Log(heightOffset);
            pelvisBone.Translate(0.0f, heightOffset, 0.0f);
        }

        /// <summary>
        /// Finds bounding box in local space. Vertex coordinates are in local space.
        /// This needs to happen manually since unity doesn't automatically recompute bounds of skinned mesh renderer after import.
        /// JL: I bet it's necessary to bake the mesh to access vertex data modified by blend shapes.
        /// AB: Yes, this is the case.
        /// </summary>
        void RecomputeLocalBounds() {
            
            // if (!skinnedMeshRenderer.sharedMesh.isReadable) {
            //     Application.Quit();
            //     throw new ArgumentException($"{skinnedMeshRenderer.gameObject.name} Mesh is not readable. Make sure to enable read/write on mesh import settings");
            // }
            
            Mesh newMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(newMesh);

            if (newMesh.vertices.Length == 0)
            {
                Debug.LogError($"no vertices in baked mesh");
                return;
            }

            Debug.Log(newMesh.vertices.Length);
            
            float xMin = Mathf.Infinity;
            float yMin = Mathf.Infinity;
            float zMin = Mathf.Infinity;
            
            float xMax = Mathf.NegativeInfinity;
            float yMax = Mathf.NegativeInfinity;
            float zMax = Mathf.NegativeInfinity;

            foreach (Vector3 vertex in newMesh.vertices) {
                Debug.Log(vertex);
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
