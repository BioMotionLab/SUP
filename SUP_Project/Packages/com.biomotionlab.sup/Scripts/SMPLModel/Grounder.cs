using System;
using JetBrains.Annotations;
using Playback;
using UnityEngine;

namespace SMPLModel {
    
    [Serializable]
    public class Grounder {
        
        static CommonGround commonGround;
        
        readonly SMPLCharacter smplCharacter;
        readonly SkinnedMeshRenderer skinnedMeshRenderer;

        public float individualFootOffset = 0;
        public float customOffset = 0;
        
        [PublicAPI]
        public float commonFootOffset;
        
        public Grounder(SMPLCharacter smplCharacter, SkinnedMeshRenderer skinnedMeshRenderer) {
            this.smplCharacter = smplCharacter;
            this.skinnedMeshRenderer = skinnedMeshRenderer;
            
            PlaybackEventSystem.OnStopAllAnimations += ResetCommonGround;
            smplCharacter.Events.OnBodyChanged += ResetCommonGround;
        }

        public void InitGround() {
            individualFootOffset = CalculateGround();
            UpdateCommonGround();
        }

        float CalculateGround() {
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            Vector3[] vertices = bakedMesh.vertices;

            float miny = Mathf.Infinity;
            foreach (Vector3 vertex in vertices) {
                miny = Mathf.Min(vertex.y, miny);
            }
            
            Transform pelvis = skinnedMeshRenderer.bones[smplCharacter.Model.PelvisIndex];
            Vector3 worldVector = pelvis.parent.TransformPoint(new Vector3(0, miny, 0));

            float currentFeetOffset = -worldVector.y;
            return currentFeetOffset;
        }

        void UpdateCommonGround() {
            if (commonGround == null) commonGround = new CommonGround(individualFootOffset);
            commonFootOffset = commonGround.Offset;
        }

        public Vector3 ApplyGround(Vector3 finalTrans, bool firstFrame) {
            
            if (firstFrame) {
                finalTrans.y += individualFootOffset;
                return finalTrans;
            }
            
            float appliedOffset = 0;
            
            switch (smplCharacter.RenderOptions.GroundSnap) {
                case GroundSnapType.None:
                    break;
                case GroundSnapType.Common:
                    appliedOffset = commonGround?.Offset ?? individualFootOffset;
                    break;
                case GroundSnapType.Individual:
                    appliedOffset = individualFootOffset;
                    break;
                case GroundSnapType.CustomValueUnityEditorOnly:
                   appliedOffset = customOffset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            finalTrans.y += appliedOffset;
            return finalTrans;
        }

        public void ResetCommonGround() {
            commonGround = null;
        }

        class CommonGround {
            public float Offset { get; }

            public CommonGround(float feetOffset) {
                Offset = feetOffset;
            }
            
        }

        public void UpdateGround() {
            individualFootOffset += CalculateGround();
        }

        public void Destory() {
            PlaybackEventSystem.OnStopAllAnimations -= ResetCommonGround;
            smplCharacter.Events.OnBodyChanged -= ResetCommonGround;
        }
    }
}