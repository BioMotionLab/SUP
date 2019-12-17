namespace MoshPlayer.Scripts.BML.SMPLModel {
    public static class SMPLConstants {
        
        public const float UnityBlendShapeScaleFactor = 100f;
        
        public const int FirstJointIndexAfterPelvis = 1;
        
        /// <summary>
        /// Represents the 3 dimensions of a vector3 (X, Y, Z) for looping purposes
        /// </summary>
        public const int DimensionsOfAVector3 = 3;

        public const int RotationMatrixElementCount = 9;
        
        public static class JSONKeys {
            public const string JointTemplates      = "template_J";
            public const string BetaJointRegressors = "betasJ_regr";
        }
        
        
    }
}