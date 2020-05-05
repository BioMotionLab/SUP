﻿using System.Linq;
using MoshPlayer.Scripts.FileLoaders;
using UnityEngine;

namespace MoshPlayer.Scripts.SMPLModel {
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
    public class IndividualizedBody : MonoBehaviour {
        
        SkinnedMeshRenderer skinnedMeshRenderer;
        ModelDefinition model;
        JointRegressor jointRegressor;

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        float[] bodyShapeBetas;
        float[] lastFrameBetas;

        MoshCharacter moshCharacter;
        

        AverageBody averageBody;
        Vector3[] updatedVertices;
        float minimumYVertex;
        Vector3 pelvisResetPosition;
        public Vector3 pelvisNewLocation;

        void OnEnable() {
            moshCharacter = GetComponentInParent<MoshCharacter>();
            model = moshCharacter.Model;

            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            
            skinnedMeshRenderer.bones[model.PelvisIndex].localPosition = Vector3.zero;
            
            averageBody = new AverageBody(skinnedMeshRenderer, model);
            //SetFeetOnGround2();
            
            //pre-create arrays for optimization.
            bodyShapeBetas = new float[model.BodyShapeBetaCount];
            updatedVertices = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            
            
            jointRegressor = SMPLHRegressorFromJSON.LoadRegressorFromJSON(model.RegressorFile(moshCharacter.Gender));

            pelvisResetPosition = skinnedMeshRenderer.bones[model.PelvisIndex].localPosition;

        }
        
        void OnDisable() {
            averageBody.Restore();
        }
        
        public void UpdateBodyWithBetas(float[] betas) {
            bodyShapeBetas = betas;
            UpdateBody();
        }
        

        [ContextMenu("Update With Current Betas")]
        public void UpdateBody() {
            
            // Need to start with fresh body, since everything is calculated relative to it.
            averageBody.Restore();
            SetBindPoses();

            float[] savedBetas = (float[])bodyShapeBetas.Clone();
            
            //if showing averaged body, set betas to zero;
            if (!moshCharacter.RenderOptions.ShowIndividualizedBody) {
                bodyShapeBetas = new float[bodyShapeBetas.Length];
            }

            AdjustBonePositions();
            AdjustMeshToNewBones();
        
            UpdateBodyShapeBlendshapes(bodyShapeBetas);
            SetBindPoses();
            
            

            if (lastFrameBetas == null || !Enumerable.SequenceEqual(lastFrameBetas, bodyShapeBetas)) {
                moshCharacter.Events.BroadcastBodyChange();
            }
            lastFrameBetas = (float[]) bodyShapeBetas.Clone();
            
            
            //restoreBetas to actual values;
            if (!moshCharacter.RenderOptions.ShowIndividualizedBody) {
                bodyShapeBetas = savedBetas;
            }
           
        }

        /// <summary>
        /// Sets up the bone positions for the individualized body.
        /// After this the skeleton should be correct, but with a bad mesh on it.
        /// 
        /// It's important to take the Raw Bone Positions (do not center them)
        /// since the mesh will eventually deform to match.
        /// The pelvis can be re-centered afterwards
        /// </summary>
        void AdjustBonePositions()
        {
            Vector3[] newJointPositions = jointRegressor.JointPositionFrom(model, bodyShapeBetas);
            pelvisNewLocation = newJointPositions[model.PelvisIndex];
            Bones.SetPositionDownwardsThroughHierarchy(skinnedMeshRenderer.bones[model.PelvisIndex], 
                                                 skinnedMeshRenderer.transform, 
                                                 newJointPositions);
        }

        /// <summary>
        /// This adjusts the average mesh to be attached to the new bones.
        /// Result is still NOT the correct individualized mesh,
        /// just the average mesh that's been skinned to updated bone locations.
        ///
        /// This is an EXPENSIVE call. I've optimized it heavily. It seems that the biggest slowdown now is the call to
        /// BakeMesh(), which takes almost 15ms. The rest of the code takes around 3ms by my calculations.
        /// </summary>
        void AdjustMeshToNewBones() {
            
            //vertices doesn't return the actual current vertexes, it's the vertexes before skinning.
            //You need to bake the skinned mesh into a mesh object to retrieve the deformed vertices
            //Mesh bakedMesh = new Mesh();
            //skinnedMeshRenderer.BakeMesh(bakedMesh);
            
            
            // Important Optimization! Needs to cache vertex arrays since these calls are VERY expensive.
            // Not doing this will decrease FPS by 1000x
            Vector3[] sharedMeshVertices = skinnedMeshRenderer.sharedMesh.vertices;
            //Vector3[] bakedMeshVertices = bakedMesh.vertices;
            
            
            //For optimization purposes, this minY calculation needs to occur during this loop.
            minimumYVertex = Mathf.Infinity;
            
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.vertexCount; i++) {
                Vector3 correctedVertex = sharedMeshVertices[i];
                //These functions are run in same loop for heavy optimization.
                //correctedVertex = AccountForUnwantedLinearBlendSkinning(sharedMeshVertices[i], bakedMeshVertices[i], averageBody.Vertices[i]);

                correctedVertex = CorrectMeshToRigOffset(correctedVertex);
                updatedVertices[i] = correctedVertex;
                
                
                // Calculate Minimum Y here, to avoid looping through all ~7000 vertices again later.
                minimumYVertex = Mathf.Min(minimumYVertex, correctedVertex.y);
            }
            
            skinnedMeshRenderer.sharedMesh.vertices = updatedVertices;

        }
        
        /// <summary>
        /// This deforms the average mesh to the correct individualized mesh based on the body-shape betas.
        /// </summary>
        void UpdateBodyShapeBlendshapes(float[] betas) {
            for (int betaIndex = 0; betaIndex < model.BodyShapeBetaCount; betaIndex++) {
                float scaledBeta = ScaleBlendshapeFromBlenderToUnity(betas[betaIndex]);
                skinnedMeshRenderer.SetBlendShapeWeight(betaIndex, scaledBeta);
            }
        }

        /// <summary>
        /// The some of the models have a scaling factor for some reason (e.g. SMPL is 1/5, SMPLH is 1).
        /// Blendshapes in unity are scaled 0-100 rather than 0-1, so also need to correct for that. 
        /// </summary>
        float ScaleBlendshapeFromBlenderToUnity(float rawWeight) {
            float scaledWeight = rawWeight * model.ShapeBlendShapeScalingFactor * model.UnityBlendShapeScaleFactor;
            return scaledWeight;
        }
        

        /// <summary>
        /// Correct for an error in the FBX construction
        /// where the mesh and bones have different origins.
        /// This makes sure the skeleton is not offset from the body
        ///
        /// this is heavily optimized to reduce frame rate lag caused by garbage collection.
        /// the combined offset is actually two offsets added together, but even simple vector addition
        /// was slowing it down. Now I precompute the addition since it stays constant.
        /// </summary>
        Vector3 CorrectMeshToRigOffset(Vector3 vertex) {
            return vertex - moshCharacter.MeshCorrection.CombinedOffset;
        }
        
        /// <summary>
        /// Used to move pelvis upwards to plant feet on ground based on the lowest Y vertex in the Mesh.
        /// </summary>
        [ContextMenu("reground")]
        void SetFeetOnGround() {
            float offsetFromGround = minimumYVertex + moshCharacter.MeshCorrection.OffsetErrorBetweenPelvisAndZero.y + pelvisResetPosition.y - pelvisNewLocation.y;
            Vector3 offsetFromGroundVector = new Vector3(0, offsetFromGround, 0);
            //Debug.Log($"offset: {offsetFromGround.ToString("f4")}");
            Transform pelvis = skinnedMeshRenderer.bones[model.PelvisIndex];
            pelvis.localPosition = offsetFromGroundVector;
        }

        void SetBindPoses()
        {
            Matrix4x4[] bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;

            Transform avatarRootTransform = skinnedMeshRenderer.transform.parent;

            Transform[] bones = skinnedMeshRenderer.bones;
            for (int i=0; i < bones.Length; i++)
            {
                // The bind pose is bone's inverse transformation matrix.
                // Make this matrix relative to the avatar root so that we can move the root game object around freely.            
                bindPoses[i] = bones[i].worldToLocalMatrix * avatarRootTransform.localToWorldMatrix;
            }

            
            Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
            sharedMesh.bindposes = bindPoses;
            skinnedMeshRenderer.sharedMesh = sharedMesh;
        }
    }
}