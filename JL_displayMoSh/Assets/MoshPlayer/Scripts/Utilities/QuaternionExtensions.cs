using JetBrains.Annotations;
using UnityEngine;

namespace MoshPlayer.Scripts.Utilities {
    public static class QuaternionExtensions {

        /// <summary>
        /// Convert rotation Quaternion to 3x3 rotation matrix, converted to RHS so blend shapes work correctly.
        /// </summary>
        /// <param name="quaternion">Quaternion to convert to a matrix.</param>
        /// <returns></returns>
        [PublicAPI]
        public static float[] To3X3MatrixMinusIdent(this Quaternion quaternion)
        {
            //convert to from unity back to MPI's maya coordinate system
            Quaternion rightHandedQuaternion = quaternion.ToRightHanded();

            float x = rightHandedQuaternion.x;
            float y = rightHandedQuaternion.y;
            float z = rightHandedQuaternion.z;
            float w = rightHandedQuaternion.w ;

            float xx      = x * x;
            float xy      = x * y;
            float xz      = x * z;
            float xw      = x * w;
            float yy      = y * y;
            float yz      = y * z;
            float yw      = y * w;
            float zz      = z * z;
            float zw      = z * w;
        
            float[] rot3X3 = new float[9];
            rot3X3[0] = 1 - 2 * ( yy + zz );
            rot3X3[1] = 2 * ( xy - zw );
            rot3X3[2] = 2 * ( xz + yw );

            rot3X3[3] = 2 * ( xy + zw );
            rot3X3[4] = 1 - 2 * ( xx + zz );
            rot3X3[5] = 2 * ( yz - xw );

            rot3X3[6] = 2 * ( xz - yw );
            rot3X3[7] =  2 * ( yz + xw );
            rot3X3[8] = 1 - 2 * ( xx + yy );
            
            //subtract ident because life is hard.
            rot3X3[0] -= 1;
            rot3X3[4] -= 1;
            rot3X3[8] -= 1;

            return rot3X3;
        }
        
        /// <summary>
        /// From https://www.gamedev.net/forums/topic/654682-quaternions-convert-between-left-right-handed-without-using-euler/
        /// </summary>
        /// <param name="leftHandedQuaternion"></param>
        /// <returns></returns>
        [PublicAPI]
        public static Quaternion ToRightHanded(this Quaternion leftHandedQuaternion) {
            // Blasted left-handed coordinate system -- Converting quaternions from LHS to RHS so that pose blendshapes get the correct values

            // if the difference is just handedness, I would have thought only 1 axis would need to be flipped. But these are quaternions, and 
            // pretty much no one understands quaternions. It like - violates labor laws or something.
        
            Quaternion rightHandedQuaternion = new Quaternion (-leftHandedQuaternion.x,
                                                               -leftHandedQuaternion.z,
                                                               leftHandedQuaternion.y,
                                                               -leftHandedQuaternion.w);
            return rightHandedQuaternion;
        }
        
        [PublicAPI]
        public static Quaternion ToLeftHanded(this Quaternion inMayaCoords) {
            float x = -inMayaCoords.x;
            float y = inMayaCoords.y;
            float z = inMayaCoords.z;
            float w = -inMayaCoords.w;
            Quaternion inUnityCoords = new Quaternion(x, y, z, w);
            return inUnityCoords;
        }


    }
}
