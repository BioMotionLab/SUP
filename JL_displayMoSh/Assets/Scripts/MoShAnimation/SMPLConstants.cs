public static class SMPLConstants {
    // a scale variable is needed in order to calculate the beta values.
    public const float BetaScalingFactor = 5.0f;

    // these should be fixed to be more consistent. 
    public const int BetaCount              = 10;
    public const int JointCount             = 24;
    public const int PoseCount              = 207;
    public const int DoubledShapeBlendCount =  BetaCount * 2;
    const        int DoubledPoseBlendCount  = PoseCount * 2;
    const        int DoubledBlendCount      = DoubledShapeBlendCount + DoubledPoseBlendCount;
    
    public const bool ZAxisUp = true;

}