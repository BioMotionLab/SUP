using JetBrains.Annotations;
using UnityEngine;

namespace MoshPlayer.Scripts.Utilities {
    public static class QuaternionExtensions {
        
        /// <summary>
        /// Convert rotation Quaternion to 3x3 rotation matrix.
        /// </summary>
        [PublicAPI]
        public static float[] To3X3Matrix(this Quaternion quaternion)
        {

            float x = quaternion.x;
            float y = quaternion.y;
            float z = quaternion.z;
            float w = quaternion.w ;
            
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
            rot3X3[0] = 1 - 2*yy - 2*zz;
            //rot3X3[0] =     2*yy - 2*zz;
            
            rot3X3[1] =     2*xy - 2*zw ;
            rot3X3[2] =     2*xz + 2*yw;

            rot3X3[3] =     2*xy + 2*zw;
            
            rot3X3[4] = 1 - 2*xx - 2*zz;
            //rot3X3[4] =     2*xx - 2*zz;
            
            rot3X3[5] =     2*yz - 2*xw;

            rot3X3[6] =     2*xz - 2*yw;
            rot3X3[7] =     2*yz + 2*xw;
            
            rot3X3[8] = 1 - 2*xx - 2*yy;
            //rot3X3[8] =     2*xx - 2*yy;
            
            return rot3X3;
        }

        public static float[] Convert3X3MatrixNeg(this float[] orig) {
            float[] newone = (float[])orig.Clone();
            newone[2] *= -1;
            newone[5] *= -1;
            newone[6] *= -1;
            newone[7] *= -1;
            return newone;
        }
        
        /// <summary>
        /// From https://www.gamedev.net/forums/topic/654682-quaternions-convert-between-left-right-handed-without-using-euler/
        /// </summary>
        [PublicAPI]
        public static Quaternion ToRightHanded(this Quaternion leftHandedQuaternion) {
            Quaternion rightHandedQuaternion = new Quaternion (-leftHandedQuaternion.x,
                                                               leftHandedQuaternion.y,
                                                               leftHandedQuaternion.z,
                                                               -leftHandedQuaternion.w);
            return rightHandedQuaternion;
        }
        
        public static Quaternion ToRightHanded2(this Quaternion leftHandedQuaternion) {
            Debug.Log($"left {leftHandedQuaternion.eulerAngles.ToString("F4")}");
            Quaternion rightHandedQuaternion = new Quaternion (-leftHandedQuaternion.z,
                                                               leftHandedQuaternion.x,
                                                               -leftHandedQuaternion.y,
                                                               leftHandedQuaternion.w);
            //rightHandedQuaternion = Quaternion.Inverse(rightHandedQuaternion);
            Debug.Log($"right {rightHandedQuaternion.eulerAngles.ToString("F4")}");
            return rightHandedQuaternion;
        }
        
        
       

        
        [PublicAPI]
        public static Quaternion ToLeftHanded(this Quaternion rightHanded) {
            float x = -rightHanded.x;
            float y = rightHanded.y;
            float z = rightHanded.z;
            float w = -rightHanded.w;
            Quaternion inUnityCoords = new Quaternion(x, y, z, w);
            return inUnityCoords;
        }
        
     


    }
}
