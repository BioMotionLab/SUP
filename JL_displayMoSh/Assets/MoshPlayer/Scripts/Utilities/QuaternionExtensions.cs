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
        
        /// <summary>
        /// From https://www.gamedev.net/forums/topic/654682-quaternions-convert-between-left-right-handed-without-using-euler/
        /// </summary>
        [PublicAPI]
        public static Quaternion ToRightHanded(this Quaternion leftHandedQuaternion) {
            // Blasted left-handed coordinate system -- Converting quaternions from LHS to RHS so that pose blendshapes get the correct values

            // if the difference is just handedness, I would have thought only 1 axis would need to be flipped. But these are quaternions, and 
            // pretty much no one understands quaternions. It like - violates labor laws or something.

            Quaternion rightHandedQuaternion = new Quaternion (-leftHandedQuaternion.x,
                                                               leftHandedQuaternion.y,
                                                               leftHandedQuaternion.z,
                                                               -leftHandedQuaternion.w);
            return rightHandedQuaternion;
        }
        
        public static Quaternion ToRightHanded2(this Quaternion leftHandedQuaternion) {
            // Blasted left-handed coordinate system -- Converting quaternions from LHS to RHS so that pose blendshapes get the correct values

            // if the difference is just handedness, I would have thought only 1 axis would need to be flipped. But these are quaternions, and 
            // pretty much no one understands quaternions. It like - violates labor laws or something.
            Debug.Log($"left {leftHandedQuaternion.eulerAngles.ToString("F4")}");
            Quaternion rightHandedQuaternion = new Quaternion (-leftHandedQuaternion.y,
                                                               -leftHandedQuaternion.z,
                                                               -leftHandedQuaternion.z,
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
