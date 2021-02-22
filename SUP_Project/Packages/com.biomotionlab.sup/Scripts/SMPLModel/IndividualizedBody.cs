using System;
using System.Linq;
using FileLoaders;
using UnityEngine;

namespace SMPLModel {
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
        Vector3 pelvisNewLocation;

        void Awake() {
            moshCharacter = GetComponent<MoshCharacter>();
            model = moshCharacter.Model;
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

            skinnedMeshRenderer.bones[model.PelvisIndex].localPosition = Vector3.zero;
            
            averageBody = new AverageBody(skinnedMeshRenderer, model);
            //SetFeetOnGround2();
            
            //pre-create arrays for optimization.
            bodyShapeBetas = new float[model.BodyShapeBetaCount];
            updatedVertices = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            
            
            jointRegressor = SMPLHRegressorFromJSON.LoadRegressorFromJSON(model.RegressorFile(moshCharacter.Gender));

            pelvisResetPosition = skinnedMeshRenderer.bones[model.PelvisIndex].localPosition;

        }
        
        void OnDestroy() {
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

            if (lastFrameBetas == null || !lastFrameBetas.SequenceEqual(bodyShapeBetas)) {
                moshCharacter.Events.BodyHasChanged();
            }
            lastFrameBetas = (float[]) bodyShapeBetas.Clone();
            
            //restore Betas to actual values saved above;
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
        /// This is an EXPENSIVE call. I've optimized it heavily.
        /// </summary>
        void AdjustMeshToNewBones() {

            // Important Optimization! Needs to cache vertex arrays since these calls are VERY expensive.
            // Not doing this will decrease FPS by 1000x
            Vector3[] sharedMeshVertices = skinnedMeshRenderer.sharedMesh.vertices;

            //For optimization purposes, this minY calculation needs to occur during this loop.
            minimumYVertex = Mathf.Infinity;
            
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.vertexCount; i++) {
                Vector3 correctedVertex = sharedMeshVertices[i];
                
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

        public void SetDebugBetas() {
            moshCharacter.RenderOptions.ShowIndividualizedBody = true;
            Debug.LogWarning("Mode Enabled.");
            float[] debugBetas = new float[16];
            switch (moshCharacter.Gender) {
                case Gender.Male:
                    debugBetas = new[] {13, -4.4f, 2.62f, -4.38f, 0.64f, 0.58f, 0,0,0,0,0,0,0,0,0,0};
                    break;
                case Gender.Female:
                    debugBetas = new[] {-13.4f, -3.43f, 0f, 5f, 1.8f, -1.47f, -.42f, -.12f, 11f, -.42f, 3f, .68f,-2.2f, 0,0,0 };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            UpdateBodyWithBetas(debugBetas);
        }
    }
}