using UnityEngine;

namespace MoshPlayer.Scripts.Utilities {
    
    public static class Vector3Extensions {
        
        /// <summary>
        /// Joseph Landy:
        /// original code has x flipped, because Unity has it's z axis flipped
        /// compared to other software. I don't know why this would require flipping the x axis.
        /// This might be an error. Oh... this might be because the object was rotated earlier.
        /// possibly worth investigating.
        /// I feel like some of the flips and rotations might be redundant, but it's a bit risky breaking them!
        /// </summary>
        public static Vector3 ToLeftHanded(this Vector3 rightHanded) {
            float x = -rightHanded.x;
            float y = rightHanded.y;
            float z = rightHanded.z;
            Vector3 leftHandedVector = new Vector3(x, y, z);
            return leftHandedVector;
        }
    }
}