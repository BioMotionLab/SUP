using UnityEngine;

namespace SMPL.Scripts.mpi {
    
    public static class QuaternionExtension {
	
        public static Quaternion ToRightHanded(this Quaternion leftHandedQuaternion) {
            float x = leftHandedQuaternion.x * -1.0f;
            float y = leftHandedQuaternion.y * 1.0f;
            float z = leftHandedQuaternion.z * 1.0f;
            float w = leftHandedQuaternion.w * -1.0f;
            Quaternion rightHandedQuaternion = new Quaternion(x, y, z, w);
            return rightHandedQuaternion;
        }
	
        public static float[] To3X3Matrix (this Quaternion q) {

            float[] rot3X3 = new float[9];

            // Note: the -1 in indices 0, 4 & 8 are the rotation-np.eye(3) for pose-mapping of SMPL model
            rot3X3[0] = (2 * q.y * q.y) - (2 * q.z * q.z);
            rot3X3[1] = (2 * q.x * q.y) - (2 * q.z * q.w);
            rot3X3[2] = (2 * q.x * q.z) + (2 * q.y * q.w);

            rot3X3[3] = (2 * q.x * q.y) + (2 * q.z * q.w);
            rot3X3[4] = (2 * q.x * q.x) - (2 * q.z * q.z);
            rot3X3[5] = (2 * q.y * q.z) - (2 * q.x * q.w);

            rot3X3[6] = (2 * q.x * q.z) - (2 * q.y * q.w);
            rot3X3[7] = (2 * q.y * q.z) + (2 * q.x * q.w);
            rot3X3[8] = (2 * q.x * q.x) - (2 * q.y * q.y);

            return rot3X3;
        }
    }
}