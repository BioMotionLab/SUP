using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MoshPlayer.Scripts.BML.FileLoaders;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    
    /// <summary>
    /// This modifies a body to match its individual shape based on its specified beta-parameters.
    ///
    /// It first modifies the skeleton to the correct shape, and then corrects distortions in the mesh
    /// caused by the skeleton modifications. This problem is because Unity will automatically
    /// apply linear blend skinning as soon as the bones move in any way.
    /// The MPI model does linear blend skinning manually AFTER the bones have been moved around.
    /// So, we have to correct for the automatic linear blend skinning here in Unity.
    ///
    /// Finally, it applies the shape-blendshapes to the corrected mesh
    /// so that it matches the correct mesh for the body.
    /// </summary>
    [ExecuteInEditMode]
    public class IndividualizedBody : MonoBehaviour {
        
        SkinnedMeshRenderer skinnedMeshRenderer;
        Vector3 minBounds;
        ModelDefinition model;
        JointRegressor jointRegressor;
        Vector3 newPelvisPosition;
        Mesh cachedIndividualizedMesh;

        Mesh originalMesh;
        Vector3[] originalBonePositions;


        [SerializeField]
        // ReSharper disable once InconsistentNaming
        float[] bodyShapeBetas;

        void OnEnable() {
            Debug.Log("Enabling individualized body");
            MoshCharacter moshCharacter = GetComponentInParent<MoshCharacter>();
            model = moshCharacter.Model;
            
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            CacheAverageBody();
            
            if (bodyShapeBetas == null || bodyShapeBetas.Length == 0) bodyShapeBetas = new float[model.BodyShapeBetaCount];
            
            jointRegressor = SMPLHRegressorFromJSON.LoadRegressorFromJSON(model.RegressorFile);
        }

        void CacheAverageBody() {
            if (originalBonePositions == null) {
                originalBonePositions = new Vector3[skinnedMeshRenderer.bones.Length];
                for (int boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; boneIndex++) {
                    originalBonePositions[boneIndex] = skinnedMeshRenderer.bones[boneIndex].position;
                }
            }
            
            if (originalMesh == null) originalMesh = skinnedMeshRenderer.sharedMesh;
            skinnedMeshRenderer.sharedMesh = Instantiate(originalMesh);
            
            
        }

        void OnDisable() {
            ResetToAverageBody();
        }

        [ContextMenu("Reset to average body")]
        void ResetToAverageBody() {
            bodyShapeBetas = new float[bodyShapeBetas.Length];
            UpdateBodyShapeBlendshapes();
            for (int boneIndex = 0; boneIndex < originalBonePositions.Length; boneIndex++) {
                skinnedMeshRenderer.bones[boneIndex].position = originalBonePositions[boneIndex];
            }
            skinnedMeshRenderer.sharedMesh = Instantiate(originalMesh);
            
        }

        public void SetupBodyWithBetas(float[] betas) {
            bodyShapeBetas = betas;
            UpdateBody();
        }

        [ContextMenu("Update With Current Betas")]
        public void UpdateBody() {
            if (bodyShapeBetas.Length == 0) bodyShapeBetas = new float[model.BodyShapeBetaCount];

            float[] tempStoreOfBetas = (float[])bodyShapeBetas.Clone();
            ResetToAverageBody();
            bodyShapeBetas = tempStoreOfBetas;
            
            Vector3[] bonePositions = jointRegressor.JointPositionFrom(model, bodyShapeBetas);
            
            AdjustBonePositions(bonePositions);
            AdjustMeshToNewBones();
            

            UpdateBodyShapeBlendshapes();
            
        }

        /// <summary>
        /// Sets up the bone positions for the individualized body.
        /// After this the skeleton should be correct, but with a bad mesh on it.
        /// </summary>
        /// <param name="jointPositions"></param>
        void AdjustBonePositions(Vector3[] jointPositions) {
            Vector3 initialPelvisPosition = skinnedMeshRenderer.bones[0].position;

            jointPositions = CenterAroundPosition(jointPositions, initialPelvisPosition);
            jointPositions = ConvertToUnityCoordinateSystem(jointPositions);

            SetPositionDownwardsThroughHierarchy(skinnedMeshRenderer.bones[0], jointPositions);
            
        }

        /// <summary>
        /// This adjusts the average mesh to be attached to the new bones.
        /// This is still NOT the correct individualized mesh,
        /// just the average mesh that's been skinned to updated bone locations.
        ///
        /// Since the bones are being moved, the mesh deforms automatically to
        /// compensate with linear blend skinning. However, since the blendshapes
        /// are computed relative to the average body, this presents a problem.
        /// We need to undo this automatic linear blend skinning by applying a correction.
        /// 
        /// We also correct for an error in the FBX construction
        /// where the mesh and bones have different origins.
        /// This makes sure the skeleton is not offset from the body
        /// </summary>
        void AdjustMeshToNewBones() {
            AccountForUnwantedLinearBlendSkinning();
            CorrectMeshToRigOffset();
        }


        /// <summary>
        /// This deforms the average mesh to the correct individualized mesh based on the body-shape betas.
        /// </summary>
        void UpdateBodyShapeBlendshapes() {
            for (int betaIndex = 0; betaIndex < model.BodyShapeBetaCount; betaIndex++) {
                float scaledBeta = ScaleBlendshapeFromBlenderToUnity(bodyShapeBetas[betaIndex]);
                skinnedMeshRenderer.SetBlendShapeWeight(betaIndex, scaledBeta);
            }
        }

        void ResetBodyShapeBlendshapes() {
            for (int betaIndex = 0; betaIndex < model.BodyShapeBetaCount; betaIndex++) {
                skinnedMeshRenderer.SetBlendShapeWeight(betaIndex, 0);
            }
        }

        /// <summary>
        /// The some of the models have a scaling factor for some reason (e.g. SMPL is 1/5).
        /// Blendshapes in unity are scaled 0-100 rather than 0-1, so also need to correct for that. 
        /// </summary>
        /// <param name="rawWeight"></param>
        /// <returns></returns>
        float ScaleBlendshapeFromBlenderToUnity(float rawWeight) {
            if (!model.AddShapeBlendshapes) return 0;
            float scaledWeight = rawWeight * model.ShapeBlendShapeScalingFactor * model.UnityBlendShapeScaleFactor;
            return scaledWeight;
        }


        /// <summary>
        /// Position of bones must be adjusted carefully, since moving a parent after setting the child's
        /// position will then move the child away from desired location. This function traverses through
        /// a hierarchy to ensure this never happens.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="jointPositions"></param>
        /// <param name="transformList"></param>
        static void SetPositionDownwardsThroughHierarchy(Transform       parent, Vector3[] jointPositions,
                                                         List<Transform> transformList = null) {
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

        void CorrectMeshToRigOffset() {
            Vector3[] translatedVertexes = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            for (int i = 0; i < translatedVertexes.Length; i++) {
                translatedVertexes[i] = skinnedMeshRenderer.sharedMesh.vertices[i] + model.OffsetErrorInFBXBetweenRigAndMesh;
            }

            skinnedMeshRenderer.sharedMesh.vertices = translatedVertexes;
        }

        Vector3[] CenterAroundPosition(Vector3[] jointPositions, Vector3 initialPos) {
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


        /// <summary>
        /// The linear blend skinning deforms the mesh when you modify the bones.
        /// The following code undoes that unwanted blend skinning.
        /// The body shape blendshapes (betas) were calculated as deformations to the average mesh,
        /// not a bone-deformed mesh. That is why this is necessary.
        /// </summary>
        /// <param name="oldVertexes"></param>
        void AccountForUnwantedLinearBlendSkinning() {
            
            //vertices doesn't return the actual current vertexes, it's its vertexes before skinning.
            //You need to bake the skinned mesh into a mesh object to retrieve the deformed vertices
            Vector3[] vertexesWithUnwantedSkinningRemoved = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];

            Vector3[] oldVertexes = originalMesh.vertices;
            
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            Vector3[] currentVertices = bakedMesh.vertices;
            
            for (int i = 0; i < oldVertexes.Length; i++) {
                var bakedMeshVertex = currentVertices[i];
                Vector3 differenceVector = oldVertexes[i] - bakedMeshVertex;
                //basically undoes the deformation 
                vertexesWithUnwantedSkinningRemoved[i] = skinnedMeshRenderer.sharedMesh.vertices[i] + differenceVector;
            }

            //now copy back to actual vertexes.
            skinnedMeshRenderer.sharedMesh.vertices = vertexesWithUnwantedSkinningRemoved;
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
            if (Mathf.Abs(heightOffset) > 500) {
                Debug.LogError("heightOffset calculated incorrectly");
                return;
            }

            Transform pelvisBone = skinnedMeshRenderer.bones[model.PelvisIndex];
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

            if (newMesh.vertices.Length == 0) {
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
        
        public void ResetToIndividualizedMesh() {
            if (cachedIndividualizedMesh == null) return;
            skinnedMeshRenderer.sharedMesh.vertices = cachedIndividualizedMesh.vertices;
        }
    }
}