namespace MoshPlayer.Scripts.Utilities {
    public static class MatrixUtilities {
        public static float[] RotationMatrix3x3ToRightHanded(float[] leftHandedRotationMatrix) {

            float[] converted3X3 = new float[leftHandedRotationMatrix.Length];
            //leftHandedRotationMatrix.CopyTo(converted3X3,0);
            converted3X3[0] = leftHandedRotationMatrix[0];
            converted3X3[1] = leftHandedRotationMatrix[2];
            converted3X3[2] = leftHandedRotationMatrix[1];
            converted3X3[3] = leftHandedRotationMatrix[3];
            converted3X3[4] = leftHandedRotationMatrix[5];
            converted3X3[5] = leftHandedRotationMatrix[4];
            converted3X3[6] = leftHandedRotationMatrix[6];
            converted3X3[7] = leftHandedRotationMatrix[8];
            converted3X3[8] = leftHandedRotationMatrix[7];
            return converted3X3;
        }
        
        
        public static float[] SubtractIdentity(float[] rotationMatrix) {
            float[] result3X3 = new float[rotationMatrix.Length];
            rotationMatrix.CopyTo(result3X3,0);
            //subtract ident because life is hard.
            result3X3[0] -=  1;
            result3X3[4] -=  1;
            result3X3[8] -=  1;
            return result3X3;
        }
    }
}