public static class SMPL {
    // a scale variable is needed in order to calculate the beta values.
    public const float BetaScalingFactor = 5.0f;

    // these should be fixed to be more consistent. 
    public const int ShapeBetaCount         = 10;
    public const int JointCount             = 24;
    public const int PoseCount              = 207;
    public const int DoubledShapeBetaCount  = ShapeBetaCount * 2;
    const        int DoubledPoseBlendCount  = PoseCount * 2;
    const        int DoubledBlendCount      = DoubledShapeBetaCount + DoubledPoseBlendCount;
    
    public const bool ZAxisUp = true;

    public class JSONKeys {
     
        public const string Gender    = "gender";
        public const string Male   = "male";
        public const string Female = "female";
        public const string FPS       = "fps";
        public const string Trans     = "trans";
        public const string Betas     = "betas";
        public const string Poses     = "poses";
    }

}