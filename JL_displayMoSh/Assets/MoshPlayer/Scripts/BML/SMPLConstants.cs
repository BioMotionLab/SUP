namespace MoshPlayer.Scripts.BML {
    public static class SMPLConstants {
        // a scale variable is needed in order to calculate the beta values.
        public const float BetaScalingFactor = 5.0f;

        // these should be fixed to be more consistent. 
        public const int ShapeBetaCount = 10;
        public const int JointCount     = 24;
        
        /// <summary>
        /// PoseCount is 24 joints but subtract pelvis, so 23. Then one per element of 3x3 rotation matrix (9). So 23*9 
        /// </summary>
        public const int PoseCount      = 207;

        public const bool ZAxisUpInOriginalFiles = true;
    
        /// <summary>
        /// Represents the 3 dimensions of a vector3 (X, Y, Z) for looping purposes
        /// </summary>
        public const int DimensionsOfAVector3 = 3;

        public const int RotationMatrixElementCount = 9;
    
        public static class JSONKeys {
     
            public const string Gender = "gender";
            public const string Male   = "male";
            public const string Female = "female";
            public const string FPS    = "fps";
            public const string Translation  = "trans";
            public const string Betas  = "betas";
            public const string Poses  = "poses";

            public const string JointTemplates      = "template_J";
            public const string BetaJointRegressors = "betasJ_regr";
        }

    }
}