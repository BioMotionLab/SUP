using UnityEngine;

namespace SMPLModel {
    /// <summary>
    /// This class stores the data for the average mesh, to enable easy resetting.
    /// This is especially important to restore mesh on program quit not not
    /// cause permanent changes to the mesh.
    /// </summary>
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
                //Debug.Log($"bone {bone.animationName} pos: {bone.position} saved as {originalBonePositions[boneIndex]}");
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
            
            int startingJoint =  model.FirstPoseIsPelvisTranslation ? 1 : 0;
            for (int jointIndex = startingJoint; jointIndex < model.JointCount; jointIndex++) {
                for (int rotMatrixElement = 0; rotMatrixElement < SMPLConstants.RotationMatrixElementCount; rotMatrixElement++) {
                    float scaledWeight = 0;
                    int jointIndexNoPelvis = model.FirstPoseIsPelvisTranslation ? jointIndex - 1 : jointIndex; // no blendshapes for pelvis.
                    int blendShapeIndex = model.BodyShapeBetaCount + jointIndexNoPelvis * SMPLConstants.RotationMatrixElementCount + rotMatrixElement;
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, scaledWeight);
                }
            }
            
        }
    }
}