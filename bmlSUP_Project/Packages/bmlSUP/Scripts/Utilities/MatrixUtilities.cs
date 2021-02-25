namespace Utilities {
    public static class MatrixUtilities {
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