using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MoshPlayer.Scripts.BML.FileLoaders;
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
        readonly Regressor regressor;
        Vector3[] originalVerticies;
        Vector3 initialPos;
        Vector3 newPelvisPosition;
        Vector3[] originalVerticiesCentered;
        Vector3 translationFromNewJointPositions;

        public BoneModifier(ModelDefinition model, SkinnedMeshRenderer skinnedMeshRenderer, Gender gender, float[] bodyShapeBetas, SMPLSettings settings)
        {
            this.model = model;
            moshCharacterTransform = skinnedMeshRenderer.transform.parent;
            this.skinnedMeshRenderer = skinnedMeshRenderer;
            this.settings = settings;
            bones = skinnedMeshRenderer.bones;

            regressor = SMPLHRegressorFromJSON.LoadRegressorFromJSON(model.RegressorFile);
        }
        
        
         /// <summary>
        /// Gets the new joint positions from the animation.
        /// Passes them to the boneModifier. 
        /// </summary>
        /// <param name="bodyShapeBetas"></param>
        public void SetupBones(float[] bodyShapeBetas) {
            Vector3[] jointPositions = regressor.JointPositionFrom(bodyShapeBetas);
            SetupBonePositions(jointPositions);
            if (settings.SnapMeshFeetToGround) {
                SetFeetOnGround();
            }
        }
         
         static List<Transform> GetAllChildren(Transform parent, List<Transform> transformList = null)
         {
             if (transformList == null) transformList = new List<Transform>();
             
             foreach (Transform child in parent) {
                 transformList.Add(child);
                 GetAllChildren(child, transformList);
             }
             return transformList;
         }
         
         
         static void SetPositionDownwardsThroughHierarchy(Transform parent, Vector3[] jointPositions, List<Transform> transformList = null)
         {
             if (transformList == null) transformList = new List<Transform>();
             
             string boneName = parent.name;
             if (Bones.NameToJointIndex.TryGetValue(boneName, out int boneJointIndex)) {
                 //Debug.Log($"setting: {boneName}, {jointPositions[boneJointIndex].ToString("F6")}, index: {boneJointIndex}");
             
                 parent.position = jointPositions[boneJointIndex];
             
                 foreach (Transform child in parent) {
                     transformList.Add(child);
                     SetPositionDownwardsThroughHierarchy(child, jointPositions, transformList);
                 }
                 
             }
             
         }

        void SetupBonePositions(Vector3[] jointPositions) {
            
            initialPos = bones[0].position;
            translationFromNewJointPositions = jointPositions[0];

            
            Debug.Log($"initialPos {initialPos.ToString("F6")}");
            
            
            Mesh originalMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(originalMesh);
            
            originalVerticies = new Vector3[originalMesh.vertexCount];
            for (int i = 0; i < originalMesh.vertexCount; i++) {
                originalVerticies[i] = originalMesh.vertices[i];
            }

            jointPositions = CenteredAroundPosition(jointPositions);
            jointPositions = ConvertToUnityCoordinateSystem(jointPositions);
            
            SetPositionDownwardsThroughHierarchy(bones[0], jointPositions);

            AccountForUnwantedLinearBlendSkinning(originalVerticies);
            
            AddTranslationToMesh(model.OffsetErrorInFBXBetweenRigAndMesh);
            
        }

        void AddTranslationToMesh(Vector3 translation) {
            Vector3[] translatedVerticies = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            for (int i = 0; i < translatedVerticies.Length; i++) {
                translatedVerticies[i] = skinnedMeshRenderer.sharedMesh.vertices[i] + translation;
            }
            skinnedMeshRenderer.sharedMesh.vertices = translatedVerticies;
        }

        Vector3[] CenteredAroundPosition(Vector3[] jointPositions) {
            Vector3[] newJointPositions = new Vector3[jointPositions.Length];
            for (int i = 0; i < jointPositions.Length; i++) {
                newJointPositions[i] = jointPositions[i] + initialPos;
            }

            return newJointPositions;
        }

        Vector3[] ConvertToUnityCoordinateSystem(Vector3[] jointPositions) {
            Vector3[] flippedJointPositions = new Vector3[jointPositions.Length];
            for (int index = 0; index < jointPositions.Length; index++) {
                Vector3 jointPosition = jointPositions[index];
                flippedJointPositions[index] = new Vector3(-jointPosition.x, jointPosition.y, jointPosition.z);
            }

            return flippedJointPositions;
        }

        void ResetVertices() {
            skinnedMeshRenderer.sharedMesh.vertices = originalVerticies;
        }


        /// <summary>
        /// The linear blend skinning deforms the mesh when you modify the bones.
        /// The following code undoes that unwanted blendskinning.
        /// The body shape blendshapes (betas) were calculated as deformations to the average mesh,
        /// not a bone-deformed mesh. That is why this is necessary.
        /// </summary>
        /// <param name="oldVerticies"></param>
        public void AccountForUnwantedLinearBlendSkinning(Vector3[] oldVerticies) {
            //vertices doesn't return the actual current vertexes, it's its vertexes before skinning.
            //You need to bake the skinned mesh into a mesh object to retrieve the deformed vertices
            Vector3[] lbsRemovedVerticies = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            Vector3[] currentVertices = bakedMesh.vertices;
            
            for (int i = 0; i < oldVerticies.Length; i++) {
                var bakedMeshVertex = currentVertices[i];
                Vector3 diffVect = oldVerticies[i] - bakedMeshVertex;
                //basically undoes the deformation 
                lbsRemovedVerticies[i] = skinnedMeshRenderer.sharedMesh.vertices[i] + diffVect;
            }

            //now copy back to actual verticies.
            skinnedMeshRenderer.sharedMesh.vertices = lbsRemovedVerticies;
        }


        /// <summary>
        /// Updates the bones based on new poses and translation of pelvis
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="trans"></param>
        public void UpdateBonesRotations(Quaternion[] pose, Vector3 trans)  {
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++) {
                string boneName = bones[boneIndex].name;
                if (boneName == Bones.Pelvis) continue;
                try
                {
                    int poseIndex = Bones.NameToJointIndex[boneName];
                    bones[boneIndex].localRotation = pose[poseIndex];
                }
                catch (KeyNotFoundException)
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
            Debug.Log($"heightOffset: {heightOffset}");
            if (Mathf.Abs(heightOffset) > 500)
            {
                Debug.LogError("heightOffset calculated incorrectly");
                return;
            }

            Transform pelvisBone = bones[model.PelvisIndex];
            //Debug.Log(pelvisBone.name);
            //Debug.Log(pelvisBone.position);
            //Debug.Log(heightOffset);
            pelvisBone.position -= new Vector3(0, heightOffset, 0);

            //pelvisBone.position -= model.OffsetErrorInFBXBetweenRigAndMesh;
            //pelvisBone.
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

            //Debug.Log(newMesh.vertices.Length);
            
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
