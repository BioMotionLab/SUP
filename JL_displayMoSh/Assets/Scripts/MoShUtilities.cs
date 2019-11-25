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
        // Blasted left-handed coordinate system -- Converting quaternions from LHS to RHS so that pose blendshapes get the correct values

        // if the difference is just handedness, I would have thought only 1 axis would need to be flipped. But these are quaternions, and 
        // pretty much no one understands quaternions. It like - violates labor laws or something.
        float qx = quaternion.x * -1.0f;
        float qy = quaternion.z * -1.0f;
        float qz = quaternion.y * 1.0f;
        float qw = quaternion.w * -1.0f;
        float[] rot3X3 = new float[9];
        rot3X3[0] = (2 * qy * qy) - (2 * qz * qz);
        rot3X3[1] = (2 * qx * qy) - (2 * qz * qw);
        rot3X3[2] = (2 * qx * qz) + (2 * qy * qw);

        rot3X3[3] = (2 * qx * qy) + (2 * qz * qw);
        rot3X3[4] = (2 * qx * qx) - (2 * qz * qz);
        rot3X3[5] = (2 * qy * qz) - (2 * qx * qw);

        rot3X3[6] = (2 * qx * qz) - (2 * qy * qw);
        rot3X3[7] = (2 * qy * qz) + (2 * qx * qw);
        rot3X3[8] = (2 * qx * qx) - (2 * qy * qy);

        return rot3X3;
    }

    /// <summary>
    /// Check if a path is relative to the project, or an absolute path. 
    /// If it can be made relative, it's made relative. Otherwise the absolute
    /// path is returned. 
    /// </summary>
    /// <returns>The relative or absolute.</returns>
    /// <param name="path">Path.</param>
    public static string PathRelativeOrAbsolute(string path)
    {
        // in editor this will be path to Assets folder. 
        string appDataPath = Application.dataPath;
        // if path of the form .../Assets/Resources/x...
        // -> return Assets/Resources/x... relative path. 
        // else 
        // -> return path. Absolute path.
        if (path.StartsWith(appDataPath)) {
            // then I can safely cut everything up to assets. 
            // I'm focusing on editor functionality anyway.
            int i = path.IndexOf("Assets");
            return path.Substring(i);
        }
        return path;
    }

	
}
