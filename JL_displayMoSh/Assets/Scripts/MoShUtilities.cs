using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class MoShUtilities {

    

    /// <summary>
    /// Convert rotation Quaternion to 3x3 rotation matrix, converted to RHS so blend shapes work correctly.
    /// </summary>
    /// <param name="quaternion">Quaternion to convert to a matrix.</param>
    /// <returns></returns>
    public static float[] QuaternionTo3X3Matrix(Quaternion quaternion)
    {
        //convert to from unity back to MPI's maya coordinate system
        Quaternion rightHandedQuaternion = ToRightHandedQuaternion(quaternion);

        float X = rightHandedQuaternion.x;
        float Y = rightHandedQuaternion.y;
        float Z = rightHandedQuaternion.z;
        float W = rightHandedQuaternion.w ;

        float xx      = X * X;
        float xy      = X * Y;
        float xz      = X * Z;
        float xw      = X * W;
        float yy      = Y * Y;
        float yz      = Y * Z;
        float yw      = Y * W;
        float zz      = Z * Z;
        float zw      = Z * W;
        
        float[] rot3X3 = new float[9];
        rot3X3[0] = 1 - 2 * ( yy + zz );
        rot3X3[1] = 2 * ( xy - zw );
        rot3X3[2] = 2 * ( xz + yw );

        rot3X3[3] = 2 * ( xy + zw );
        rot3X3[4] = 1 - 2 * ( xx + zz );
        rot3X3[5] = 2 * ( yz - xw );

        rot3X3[6] = 2 * ( xz - yw );
        rot3X3[7] =  2 * ( yz + xw );
        rot3X3[8] = 1 - 2 * ( xx + yy );

        return rot3X3;
    }

    /// <summary>
    /// From https://www.gamedev.net/forums/topic/654682-quaternions-convert-between-left-right-handed-without-using-euler/
    /// </summary>
    /// <param name="leftHandedQuaternion"></param>
    /// <returns></returns>
    static Quaternion ToRightHandedQuaternion(Quaternion leftHandedQuaternion) {
        // Blasted left-handed coordinate system -- Converting quaternions from LHS to RHS so that pose blendshapes get the correct values

        // if the difference is just handedness, I would have thought only 1 axis would need to be flipped. But these are quaternions, and 
        // pretty much no one understands quaternions. It like - violates labor laws or something.
        
        Quaternion rightHandedQuaternion = new Quaternion (-leftHandedQuaternion.x,
                                                           -leftHandedQuaternion.z,
                                                           leftHandedQuaternion.y,
                                                           -leftHandedQuaternion.w);
        return rightHandedQuaternion;
    }
    
	
}
