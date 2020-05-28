using System;
using UnityEngine;

namespace MoshPlayer.Scripts.SMPLModel {
    [Serializable]
    public class MeshCorrection {
        public Vector3 OffsetErrorBetweenPelvisAndZero;
        public Vector3 OffsetErrorInFbxBetweenRigAndMesh;
        public Vector3 CombinedOffset => OffsetErrorBetweenPelvisAndZero - OffsetErrorInFbxBetweenRigAndMesh;


    }
}