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
        Mesh cachedIndividualizedMesh;

        Mesh originalMesh;
        Vector3[] originalBonePositions;


        [SerializeField]
        // ReSharper disable once InconsistentNaming
        float[] bodyShapeBetas;

        MoshCharacter moshCharacter;
        CharacterEvents events;
        private Vector3[] originalVertices;
        private Vector3 pelvisOffsetFromReshape;


        void OnEnable() {
            moshCharacter = GetComponentInParent<MoshCharacter>();
            model = moshCharacter.Model;
            events = moshCharacter.Events;
            
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            
            skinnedMeshRenderer.bones[model.PelvisIndex].localPosition = Vector3.zero;
            
            CacheAverageBody();
            //SetFeetOnGround();
            
             bodyShapeBetas = new float[model.BodyShapeBetaCount];
            
            jointRegressor = SMPLHRegressorFromJSON.LoadRegressorFromJSON(model.RegressorFile);
            
            
        }

        void CacheAverageBody()
        {
            SaveBonePositions();
            SaveVertices();
        }

        private void SaveVertices()
        {
            originalVertices = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            skinnedMeshRenderer.sharedMesh.vertices.CopyTo(originalVertices, 0);

            // Vector3[] originialCenteredVertices = new Vector3[originalVertices.Length];
            // for (int i = 0; i < originalVertices.Length; i++)
            // {
            //     originialCenteredVertices[i] = originalVertices[i] - 
            // }
            
        }

        private void SaveBonePositions()
        {
            originalBonePositions = new Vector3[skinnedMeshRenderer.bones.Length];
            for (int boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; boneIndex++)
            {
                Transform bone = skinnedMeshRenderer.bones[boneIndex];
                //Save in local coordinates of pelvis. Assumes pelvis at zero.
                originalBonePositions[boneIndex] = skinnedMeshRenderer.transform.InverseTransformPoint(bone.position);
                Debug.Log($"bone {bone.name} pos: {bone.position} saved as {originalBonePositions[boneIndex]}");
            }
        }

        void OnDisable() {
            ResetToAverageBody();
        }

        [ContextMenu("Reset to average body")]
        void ResetToAverageBody() {
            ResetBlendshapes();
            ResetBones();
            ResetMesh();
        }

        private void ResetMesh()
        {
            Vector3[] restoredVerticies = new Vector3[originalVertices.Length];
            originalVertices.CopyTo(restoredVerticies, 0);
            skinnedMeshRenderer.sharedMesh.vertices = restoredVerticies;
        }

        private void ResetBones()
        {
            for (int boneIndex = 0; boneIndex < originalBonePositions.Length; boneIndex++)
            {
                Transform bone = skinnedMeshRenderer.bones[boneIndex];
                bone.position = originalBonePositions[boneIndex];
                bone.rotation = Quaternion.identity;
            }
        }

        private void ResetBlendshapes()
        {
            float[] zeroBetas = new float[bodyShapeBetas.Length];
            UpdateBodyShapeBlendshapes(zeroBetas);
        }

        public void SetupBodyWithBetas(float[] betas) {
            bodyShapeBetas = betas;
            UpdateBody();
        }

        [ContextMenu("Update With Current Betas")]
        public void UpdateBody() {
            
            ResetToAverageBody();
            
            Vector3[] newJointPositions = jointRegressor.JointPositionFrom(model, bodyShapeBetas);
            AdjustBonePositions(newJointPositions);
            AdjustMeshToNewBones();
            
            UpdateBodyShapeBlendshapes(bodyShapeBetas);
            
            events.BroadcastBodyChange(this);
            
            //SetFeetOnGround();
        }

        /// <summary>
        /// Sets up the bone positions for the individualized body.
        /// After this the skeleton should be correct, but with a bad mesh on it.
        /// </summary>
        /// <param name="newJointPositions"></param>
        void AdjustBonePositions(Vector3[] newJointPositions)
        {
            Debug.Log("\npositions");
            Array.ForEach(newJointPositions, (ele)=> Debug.Log(ele));
            pelvisOffsetFromReshape = newJointPositions[model.PelvisIndex];
            
            Vector3[] centeredJointPositions = CenterAroundPosition(newJointPositions, pelvisOffsetFromReshape);
            
            Debug.Log("\ncentered");
            Array.ForEach(newJointPositions, (ele)=> Debug.Log(ele));
            
            SetPositionDownwardsThroughHierarchy(skinnedMeshRenderer.bones[model.PelvisIndex], 
                skinnedMeshRenderer.transform, 
                centeredJointPositions);
        }

        /// <summary>
        /// This adjusts the average mesh to be attached to the new bones.
        /// Result is still NOT the correct individualized mesh,
        /// just the average mesh that's been skinned to updated bone locations.
        /// </summary>
        void AdjustMeshToNewBones() {
            AccountForUnwantedLinearBlendSkinning();
            CorrectMeshToRigOffset();
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
            if (!model.AddShapeBlendshapes) return 0;
            float scaledWeight = rawWeight * model.ShapeBlendShapeScalingFactor * model.UnityBlendShapeScaleFactor;
            return scaledWeight;
        }


        /// <summary>
        /// Position of bones must be adjusted carefully, since moving a parent after setting the child's
        /// position will then move the child away from desired location. This function traverses through
        /// a hierarchy to ensure this never happens.
        /// </summary>
        /// <param name="parentBone"></param>
        /// <param name="rootCoordinateTransform"></param>
        /// <param name="jointPositions"></param>
        void SetPositionDownwardsThroughHierarchy(Transform parentBone, Transform rootCoordinateTransform, Vector3[] jointPositions) {

            string boneName = parentBone.name;
            if (Bones.NameToJointIndex.TryGetValue(boneName, out int boneJointIndex)) {
                //Debug.Log($"setting: {boneName}, {jointPositions[boneJointIndex].ToString("F6")}, index: {boneJointIndex}");

                parentBone.position = rootCoordinateTransform.TransformPoint(jointPositions[boneJointIndex]);
                Debug.Log($"{boneName} set to pos: {parentBone.position}");
                foreach (Transform child in parentBone) {
                    SetPositionDownwardsThroughHierarchy(child, rootCoordinateTransform, jointPositions);
                }
            }
        }

        /// <summary>
        /// Correct for an error in the FBX construction
        /// where the mesh and bones have different origins.
        /// This makes sure the skeleton is not offset from the body
        /// </summary>
        void CorrectMeshToRigOffset() {
            Vector3[] translatedVertexes = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            for (int i = 0; i < translatedVertexes.Length; i++) {
                //translatedVertexes[i] = skinnedMeshRenderer.sharedMesh.vertices[i]  + new Vector3(.00217f,-0.2407116f,0.02858f) ;
                translatedVertexes[i] = skinnedMeshRenderer.sharedMesh.vertices[i]  + new Vector3(-.00217F,-0.93,0.02858f) ;
            }
            
            //+ model.OffsetErrorInFBXBetweenRigAndMesh
            skinnedMeshRenderer.sharedMesh.vertices = translatedVertexes;
        }

        Vector3[] CenterAroundPosition(Vector3[] jointPositions, Vector3 initialPos) {
            Vector3[] newJointPositions = new Vector3[jointPositions.Length];
            for (int i = 0; i < jointPositions.Length; i++) {
                newJointPositions[i] = jointPositions[i] - initialPos;
            }

            return newJointPositions;
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
        void AccountForUnwantedLinearBlendSkinning() {
            
            //vertices doesn't return the actual current vertexes, it's its vertexes before skinning.
            //You need to bake the skinned mesh into a mesh object to retrieve the deformed vertices
            Vector3[] vertexesWithUnwantedSkinningRemoved = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            for (int i = 0; i < originalVertices.Length; i++) {
                Vector3 differenceVector = originalVertices[i] - bakedMesh.vertices[i];

                if (i % 102 == 0)
                {
                    Debug.Log($"baked differenceVector {differenceVector.ToString("F6")}");
                    Vector3 rawDifferencevector = originalVertices[i] - skinnedMeshRenderer.sharedMesh.vertices[i];
                    Debug.Log($"raw differenceVector {rawDifferencevector.ToString("F6")}");
                }
                
                
                //basically undoes the deformation 
                vertexesWithUnwantedSkinningRemoved[i] = skinnedMeshRenderer.sharedMesh.vertices[i] + differenceVector;
            }

            //now copy back to actual vertexes.
            skinnedMeshRenderer.sharedMesh.vertices = vertexesWithUnwantedSkinningRemoved;
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