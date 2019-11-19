
#undef DANGERMODE 

#undef VERTEXLOGCONTROLS

using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MoShCharacter))]
public class MoShCharacterEditor : Editor {



    MoShCharacter player;

    SerializedProperty p_vertexSelPath;
    SerializedProperty p_vlogPath;
    SerializedProperty p_jlogPath;

    SerializedProperty p_jointsRelativeTo;
    SerializedProperty p_vertsRelativeTo;

    void OnEnable()
    {
        player = target as MoShCharacter;
        p_vertexSelPath = serializedObject.FindProperty("SelectionFilePath");
        p_vlogPath = serializedObject.FindProperty("VertLogPath");
        p_jlogPath = serializedObject.FindProperty("JointLogPath");

        p_jointsRelativeTo = serializedObject.FindProperty("JointsRelativeTo");
        p_vertsRelativeTo = serializedObject.FindProperty("VerticesRelativeTo");
    }

    public override void OnInspectorGUI()
    {

        loggingControls();

#if DANGERMODE

        if (GUILayout.Button("compute shape female")) {
            player.EditorComputeShapePose_female();
        }

        if (GUILayout.Button("compute shape male")) {
            player.EditorComputeShapePose_male();
        }

        if (GUILayout.Button("reset shape to female base")) {
            player.editorBetas = new float[10];
            player.EditorComputeShapePose_female();
        }

        if (GUILayout.Button("reset shape to male base")) {
            player.editorBetas = new float[10];
            player.EditorComputeShapePose_male();
        }

#endif

        //if (GUILayout.Button("set to non-cloned meshes")) {
        //    //Mesh mf = player.smpl_f;
        //    //DestroyImmediate(player.smpl_f, false);
        //    //DestroyImmediate(player.smpl_m, false);
        //    //Resources.UnloadAsset(player.smpl_m);
        //    //Resources.UnloadAsset(player.smpl_f);
        //    player.smpl_f = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/SMPLmodels/f_avg_unityDoubleBlends_scale5.fbx");
        //    player.smpl_m = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/SMPLmodels/m_avg_unityDoubleBlends_scale5.fbx");
        //    player.GetComponent<SkinnedMeshRenderer>().sharedMesh = player.smpl_f;
        //}

        DrawDefaultInspector();
	}


    private void loggingControls() {

        // I don't think I should use serialized properties here, because
        // I think that would let setting the log flag be undone without removing the 
        // added components. 

#if VERTEXLOGCONTROLS


        if (!player.LoggingVerts) {
            if (GUILayout.Button("Enable Vertex Logging")) {
                player.LoggingVerts = true;
            }
        }
        else {
            
            if (GUILayout.Button("Disable Vertex Logging.")) {
                player.LoggingVerts = false;
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(p_vertexSelPath);
            if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {
                
                EditorApplication.delayCall += () => {
                    string s = EditorUtility.OpenFilePanelWithFilters("Vertex selection file", "Assets/Resources", new string[] { "JSON files", "json" });
                    if (s != string.Empty) {
                        s = MoShUtilities.PathRelativeOrAbsolute(s);
                        p_vertexSelPath.stringValue = s;
                        serializedObject.ApplyModifiedProperties();
                    }
                };
            }
            GUILayout.EndHorizontal();

            // output path.
            //GUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(p_vlogPath);
            //if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {
            //    p_vlogPath.stringValue = EditorUtility.OpenFolderPanel("Log output folder", "", "");
            //}
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(p_vlogPath);
            if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {

                EditorApplication.delayCall += () => {
                    string s = EditorUtility.OpenFolderPanel("Log output folder", "", "");
                    if (s != string.Empty) {
                        s = MoShUtilities.PathRelativeOrAbsolute(s);
                        p_vlogPath.stringValue = s;
                        serializedObject.ApplyModifiedProperties();
                    }
                };
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(p_vertsRelativeTo, new GUIContent("Relative To"));
        }
#endif

        if (!player.LoggingJoints) {
            if (GUILayout.Button("Enable Joint Logging")) {
                player.LoggingJoints = true;
            }
        }
        else {
            if (GUILayout.Button("Disable Joint Logging")) {
                player.LoggingJoints = false;
            }


            //GUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(p_jlogPath);
            //if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {
            //    p_jlogPath.stringValue = EditorUtility.OpenFolderPanel("Log output folder", "", "");
            //}
            //GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(p_jlogPath);
            if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {
                
                EditorApplication.delayCall += () => {
                    string s = EditorUtility.OpenFolderPanel("Log output folder", "", "");
                    if (s != string.Empty) {
                        s = MoShUtilities.PathRelativeOrAbsolute(s);
                        p_jlogPath.stringValue = s;
                        serializedObject.ApplyModifiedProperties();
                    }
                };
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(p_jointsRelativeTo, new GUIContent("Relative To"));
            //EditorGUILayout.PropertyField()
        }
        serializedObject.ApplyModifiedProperties();
    }
}
