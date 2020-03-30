using UnityEngine;

namespace MoshPlayer.Scripts.Utilities {
    
    public static class Vector3Extensions {
        
        /// <summary>
        /// Converts from Maya Coords to Unity coords
        /// X axis flipped negative, and z is now forward not up as in Maya.
        /// </summary>
        public static Vector3 ConvertTranslationFromMayaToUnity(this Vector3 rightHanded) {
            float x = -rightHanded.x;
            float y = rightHanded.z;
            float z = rightHanded.y;
            Vector3 leftHandedVector = new Vector3(x, y, z);
            return leftHandedVector;
        }
    }
}