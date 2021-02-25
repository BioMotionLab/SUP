using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SMPLModel {
    [Serializable]
    public class MeshCorrection {
        [FormerlySerializedAs("OffsetErrorBetweenPelvisAndZero")] public Vector3 offsetErrorBetweenPelvisAndZero;
        [FormerlySerializedAs("OffsetErrorInFbxBetweenRigAndMesh")] public Vector3 offsetErrorInFbxBetweenRigAndMesh;
        public Vector3 CombinedOffset => offsetErrorBetweenPelvisAndZero - offsetErrorInFbxBetweenRigAndMesh;


    }
}