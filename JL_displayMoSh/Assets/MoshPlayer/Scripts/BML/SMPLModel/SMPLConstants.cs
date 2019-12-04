namespace MoshPlayer.Scripts.BML.SMPLModel {
    public static class SMPLConstants {
        
        
        public const float SMPLBlendshapeScalingFactor = 1f / 5.0f;
        
        public const float UnityBlendShapeScaleFactor = 100f;
        
        public const int BodyShapeBetaCount = 10;
        public const int JointCount     = 24;
        
        /// <summary>
        /// PoseDependentBlendshapeCount is 24 joints but subtract pelvis, so 23. Then one per element of 3x3 rotation matrix (9). So 23*9 = 207 
        /// </summary>
        public const int PoseDependentBlendshapeCount      = 207;

        public const int FirstJointIndexAfterPelvis = 1;
        
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
            public const string Translations  = "trans";
            
            public const string Betas  = "rawBodyShapeWeightBetas";
            public const string Poses  = "poses";
            public const string JointTemplates      = "template_J";
            public const string BetaJointRegressors = "betasJ_regr";
        }
    }
}