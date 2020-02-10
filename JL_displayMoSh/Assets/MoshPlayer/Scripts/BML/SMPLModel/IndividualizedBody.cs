using System;
using System.Collections.Generic;
using MoshPlayer.Scripts.BML.FileLoaders;
using UnityEditor;
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
    public class IndividualizedBody : MonoBehaviour {
        
        SkinnedMeshRenderer skinnedMeshRenderer;
        ModelDefinition model;
        JointRegressor jointRegressor;
        Vector3 newPelvisPosition;

        [SerializeField]
        // ReSharper disable once InconsistentNaming
        float[] bodyShapeBetas;

        MoshCharacter moshCharacter;
        CharacterEvents events;
        
        Vector3 pelvisOffsetFromReshape;


        AverageBody averageBody;
        Vector3[] updatedVertices;

        void OnEnable() {
            moshCharacter = GetComponentInParent<MoshCharacter>();
            model = moshCharacter.Model;
            events = moshCharacter.Events;
            
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            
            skinnedMeshRenderer.bones[model.PelvisIndex].localPosition = Vector3.zero;
            
            averageBody = new AverageBody(skinnedMeshRenderer, model);
            SetFeetOnGround();
            
            //pre-create arrays for optimization.
            bodyShapeBetas = new float[model.BodyShapeBetaCount];
            updatedVertices = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            
            
            jointRegressor = SMPLHRegressorFromJSON.LoadRegressorFromJSON(model.RegressorFile(moshCharacter.Gender));
            
            
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
            
            averageBody.Restore();
            
            AdjustBonePositions();
            AdjustMeshToNewBones();
            
            UpdateBodyShapeBlendshapes(bodyShapeBetas);
            
            if (moshCharacter.SetFeetOnGround) SetFeetOnGround();
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
            Bones.SetPositionDownwardsThroughHierarchy(skinnedMeshRenderer.bones[model.PelvisIndex], 
                                                 skinnedMeshRenderer.transform, 
                                                 newJointPositions);
        }

        /// <summary>
        /// This adjusts the average mesh to be attached to the new bones.
        /// Result is still NOT the correct individualized mesh,
        /// just the average mesh that's been skinned to updated bone locations.
        /// </summary>
        void AdjustMeshToNewBones() {
            
            
            //vertices doesn't return the actual current vertexes, it's the vertexes before skinning.
            //You need to bake the skinned mesh into a mesh object to retrieve the deformed vertices
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            
            
            // Important! Needs to cache vertex arrays since these calls are VERY expensive.
            // Not doing this will decrease FPS by 1000x
            Vector3[] sharedMeshVertices = skinnedMeshRenderer.sharedMesh.vertices;
            Vector3[] bakedMeshVertices = bakedMesh.vertices;
            
            
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.vertexCount; i++) {
                
                Vector3 lbsCorrectedVertex = AccountForUnwantedLinearBlendSkinning(sharedMeshVertices[i],
                                                                                   bakedMeshVertices[i], 
                                                                                   averageBody.Vertices[i]);
                Vector3 offsetCorrectedVertex = CorrectMeshToRigOffset(lbsCorrectedVertex);
                updatedVertices[i] = offsetCorrectedVertex;
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
        /// The some of the models have a scaling factor for some reason (e.g. SMPL is 1/5).
        /// Blendshapes in unity are scaled 0-100 rather than 0-1, so also need to correct for that. 
        /// </summary>
        /// <param name="rawWeight"></param>
        /// <returns></returns>
        float ScaleBlendshapeFromBlenderToUnity(float rawWeight) {
            //TODO if (!model.AddShapeBlendshapes) return 0;
            float scaledWeight = rawWeight * model.ShapeBlendShapeScalingFactor * model.UnityBlendShapeScaleFactor;
            return scaledWeight;
        }
        

        /// <summary>
        /// Correct for an error in the FBX construction
        /// where the mesh and bones have different origins.
        /// This makes sure the skeleton is not offset from the body
        /// </summary>
        Vector3 CorrectMeshToRigOffset(Vector3 vertex) {
            // this is heavily optimized to reduce frame rate lag caused by garbage collection.
            return vertex - moshCharacter.CombinedOffset;
        }

        /// <summary>
        /// Since the bones are being moved, the mesh deforms automatically to
        /// compensate for these changes using linear blend skinning. However, since the blendshapes
        /// are computed relative to the average body, this presents a problem.
        /// We need to undo this automatic linear blend skinning by applying a correction.
        ///
        /// The body shape blendshapes (betas) were calculated as deformations to the average mesh,
        /// not the new bone-deformed mesh. That is why this is necessary.
        /// </summary>
        Vector3  AccountForUnwantedLinearBlendSkinning(Vector3 vertex, Vector3 bakedVertex, Vector3 averageVertex) {
            
            //Use This to output the correct combined offset error.
            //var message = averageVertex - bakedVertex;
            //Debug.Log(message.ToString("f8"));
            
            
            // this is heavily optimized to reduce framerate caused by garbage collection.
            // AverageVertex-bakedVertex is the deformation caused by LBS, we're adding it back on.
            return vertex + averageVertex - bakedVertex;
           
        }


        /// <summary>
        /// Finds distance between lowest mesh vertex and the ground. Used to move pelvis upwards to plant feet on ground
        /// </summary>
        [ContextMenu("reground")]
        void SetFeetOnGround() {
            
            //Set root back on ground
            Transform rootTransform = moshCharacter.gameObject.transform;
            rootTransform.position = new Vector3(rootTransform.position.x, 0, rootTransform.position.z);
            
            //save current Position
            Vector3 currentPelvisPosition = skinnedMeshRenderer.bones[model.PelvisIndex].position;
            skinnedMeshRenderer.bones[model.PelvisIndex].position = Vector3.zero;
            
            
            //bake the mesh to access vertex locations after modifications by blend shapes.
            Mesh newMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(newMesh);
            
            
            //find lowest vertex
            float yMin = Mathf.Infinity;
            foreach (Vector3 vertex in newMesh.vertices) {
                yMin = Mathf.Min(yMin, vertex.y);
            }
            Vector3 lowestVertexRelativeToMeshCenter = new Vector3(0, yMin, 0);
            
            //restore current Position
            skinnedMeshRenderer.bones[model.PelvisIndex].position = currentPelvisPosition;
            
            //add offset
            Vector3 offset = lowestVertexRelativeToMeshCenter + skinnedMeshRenderer.bones[model.PelvisIndex].localPosition;
            skinnedMeshRenderer.bones[model.PelvisIndex].localPosition -= offset;

        }
    }
}