using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class MoShUtilities {


	// from: https://answers.unity.com/questions/682932/using-generic-list-with-serializedproperty-inspect.html
	public static void LogProperties(SerializedObject so, bool includeChildren = true) {
		// Shows all the properties in the serialized object with name and type
		// You can use this to learn the structure
		so.Update();
		SerializedProperty propertyLogger = so.GetIterator();
		while(true) {
			Debug.Log("name = " + propertyLogger.name + " type = " + propertyLogger.type);
			if(!propertyLogger.Next(includeChildren)) break;
		}
	}

    /// <summary>
    /// Convert rotation Quaternion to 3x3 rotation matrix, converted to RHS so blend shapes work correctly.
    /// </summary>
    /// <param name="quat">Quaternion to convert to a matrix.</param>
    /// <returns></returns>
    public static float[] Quat_to_3x3Mat(Quaternion quat)
    {
        // Blasted left-handed coordinate system -- Converting quaternions from LHS to RHS so that pose blendshapes get the correct values

        // if the difference is just handedness, I would have thought only 1 axis would need to be flipped. But these are quaternions, and 
        // pretty much no one understands quaternions. It like - violates labor laws or something.
        float qx = quat.x * -1.0f;
        float qy = quat.z * -1.0f;
        float qz = quat.y * 1.0f;
        float qw = quat.w * -1.0f;
        float[] rot3x3 = new float[9];
        rot3x3[0] = 1 - (2 * qy * qy) - (2 * qz * qz) - 1;
        rot3x3[1] = (2 * qx * qy) - (2 * qz * qw);
        rot3x3[2] = (2 * qx * qz) + (2 * qy * qw);

        rot3x3[3] = (2 * qx * qy) + (2 * qz * qw);
        rot3x3[4] = 1 - (2 * qx * qx) - (2 * qz * qz) - 1;
        rot3x3[5] = (2 * qy * qz) - (2 * qx * qw);

        rot3x3[6] = (2 * qx * qz) - (2 * qy * qw);
        rot3x3[7] = (2 * qy * qz) + (2 * qx * qw);
        rot3x3[8] = 1 - (2 * qx * qx) - (2 * qy * qy) - 1;

        return rot3x3;
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

    /// <summary>
    /// Editors the name of the remove from hierarchy by.
    /// </summary>
    /// <param name="root">Root.</param>
    /// <param name="name">Name.</param>
    public static void EditorRemoveFromHierarchyByName(Transform root, string name)
    {
        Transform matching = root.Find(name);
        if (matching != null) {
            Object.DestroyImmediate(matching.gameObject);
        }
        if (root.childCount != 0) {
            foreach (Transform c in root) {
                EditorRemoveFromHierarchyByName(c, name);
            }
        }
    }

    /// <summary>
    /// Recursively traverse a transform hierarchy and remove all children with <paramref name="name"/>
    /// </summary>
    /// <param name="root">Root.</param>
    /// <param name="name">Name.</param>
    public static void RemoveFromHierarchyByName(Transform root, string name)
    {
        Transform matching = root.Find(name);
        if (matching != null) {
            Object.Destroy(matching.gameObject);
        }
        if (root.childCount != 0) {
            foreach (Transform c in root) {
                RemoveFromHierarchyByName(c, name);
            }
        }
    }
	
}
