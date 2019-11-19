using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class BakeJSONWindow : EditorWindow {


    //TextAsset jsontext;

    //Keyframe[] keyframes;

    public string n;



	[MenuItem("MoSh/Dev Utils Window")]
	static void open() {
		//BakeJSONWindow jsonWindow = EditorWindow.GetWindow<BakeJSONWindow> ();
        GetWindow<BakeJSONWindow>();
	}


	void OnGUI() {

//		try {
//			jsontext = (TextAsset)EditorGUILayout.ObjectField ("json to bake", null, typeof(TextAsset), false);
//		} catch (System.Exception ex) {
//			Debug.LogError ("cast to text asset failed.");
//		}
		//jsontext = (TextAsset)EditorGUILayout.ObjectField ("json to bake", jsontext, typeof(TextAsset), false);

		//if (GUILayout.Button("Bake JSON")) {
		//	if (jsontext != null) {
		//		Keyframe k0 = new Keyframe (0f, 0f);
		//		Keyframe k1 = new Keyframe (2f, 100f);
		//		AnimationCurve curve = new AnimationCurve (k0, k1);
		//		AnimationClip c = new AnimationClip ();
		//		// now I have to get the game object, and apply the curve to it. 
		//		// so: find the object. Let's just assume the object is selected. 
		//		c.SetCurve ("", typeof(SkinnedMeshRenderer), "blendShape.blnd.Shape001_pos", curve);
		//		AssetDatabase.CreateAsset (c, "Assets/GeneratedAnimations/c.anim");
		//	}
		//}


        if (GUILayout.Button ("examine properties of selected.")) {
            MoShUtilities.LogProperties(new SerializedObject(Selection.activeGameObject.transform));
		}

        n = GUILayout.TextField(n);
        if (GUILayout.Button("remove from hierarchy by name")) {
            Transform r = Selection.activeTransform;
            EditorRemoveFromHierarchyByName(r, n);
        }


        // I want to show the global position of the object.
        Transform obj = Selection.activeTransform;
        if (obj != null) {
            Vector3 p = obj.position;
            EditorGUILayout.Vector3Field("global position", p);
        }
	}


    public static void EditorRemoveFromHierarchyByName(Transform root, string name)
    {
        Transform matching = root.Find(name);
        if (matching != null) {
            DestroyImmediate(matching.gameObject);
        }
        if (root.childCount != 0) {
            foreach (Transform c in root) {
                EditorRemoveFromHierarchyByName(c, name);
            }
        }
    }


}
