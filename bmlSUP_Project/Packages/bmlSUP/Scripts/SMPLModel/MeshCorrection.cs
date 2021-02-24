using System;
using UnityEngine;

namespace SMPLModel {
    [Serializable]
    public class MeshCorrection {
        public Vector3 OffsetErrorBetweenPelvisAndZero;
        public Vector3 OffsetErrorInFbxBetweenRigAndMesh;
        public Vector3 CombinedOffset => OffsetErrorBetweenPelvisAndZero - OffsetErrorInFbxBetweenRigAndMesh;


    }
}