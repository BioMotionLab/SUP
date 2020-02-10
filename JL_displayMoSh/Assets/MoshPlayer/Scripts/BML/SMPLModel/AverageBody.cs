using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class AverageBody {
        
        readonly SkinnedMeshRenderer skinnedMeshRenderer;
        readonly ModelDefinition     model;

        Vector3[] vertices;
        Vector3[] originalBonePositions;
        readonly Vector3[] restoredVerticies;

        public Vector3[] Vertices => vertices;

        public AverageBody(SkinnedMeshRenderer skinnedMeshRenderer, ModelDefinition model)
        {
            this.skinnedMeshRenderer = skinnedMeshRenderer;
            this.model = model;
            
            restoredVerticies = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            
            SaveBonePositions();
            SaveVertices();
        }

        void SaveVertices()
        {
            vertices = new Vector3[skinnedMeshRenderer.sharedMesh.vertexCount];
            skinnedMeshRenderer.sharedMesh.vertices.CopyTo(vertices, 0);
        }

        void SaveBonePositions()
        {
            originalBonePositions = new Vector3[skinnedMeshRenderer.bones.Length];
            for (int boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; boneIndex++)
            {
                Transform bone = skinnedMeshRenderer.bones[boneIndex];
                //Save in local coordinates of pelvis. Assumes pelvis at zero.
                originalBonePositions[boneIndex] = skinnedMeshRenderer.transform.InverseTransformPoint(bone.position);
                //Debug.Log($"bone {bone.name} pos: {bone.position} saved as {originalBonePositions[boneIndex]}");
            }
        }
        
        
        [ContextMenu("Reset to average body")]
        public void Restore() {
            RestoreBlendshapes();
            RestoreBones();
            RestoreMeshVertices();
        }

        void RestoreMeshVertices()
        {
            vertices.CopyTo(restoredVerticies, 0);
            skinnedMeshRenderer.sharedMesh.vertices = restoredVerticies;
        }
         
        void RestoreBones()
        {
            Bones.ResetBonesDownwardsThroughHierarchy(skinnedMeshRenderer.bones[model.PelvisIndex], 
                                                       skinnedMeshRenderer.transform, 
                                                       originalBonePositions);
        }
        
        void RestoreBlendshapes()
        {
            for (int betaIndex = 0; betaIndex < model.BodyShapeBetaCount; betaIndex++) {
                skinnedMeshRenderer.SetBlendShapeWeight(betaIndex, 0);
            }
        }
    }
}