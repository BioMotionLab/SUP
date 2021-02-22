using UnityEngine;

namespace Utilities {
    
    public static class Vector3Extensions {
        
        /// <summary>
        /// Converts from Maya Coords to Unity coords
        /// X axis flipped negative,
        /// z axis is now forward not up as in Maya (swap y and z)
        /// z axis also flipped negative
        /// </summary>
        public static Vector3 ConvertTranslationFromMayaToUnity(this Vector3 rightHanded) {
            float x = -rightHanded.x;
            float y = rightHanded.z;
            float z = -rightHanded.y;
            Vector3 leftHandedVector = new Vector3(x, y, z);
            return leftHandedVector;
        }
    }
}