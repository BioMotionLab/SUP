using UnityEngine;

namespace SMPL.Scripts.mpi {
    public static class Vector3Extension {

        public static Vector3 ToLeftHandedCoordinateSystem(this Vector3 vector) {
            return new Vector3(-vector.x, vector.y, vector.z);
        }
		
    }
}