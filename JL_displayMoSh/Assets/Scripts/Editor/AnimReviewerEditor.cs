using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimReviewer))]
public class AnimReviewerEditor : Editor {

    SerializedProperty p_animListPath;
    SerializedProperty p_AnimFolder;
    SerializedProperty p_responseFilePath;

	void OnEnable()
	{
        p_animListPath = serializedObject.FindProperty("animListPath");
        p_AnimFolder = serializedObject.FindProperty("AnimFolder");
        p_responseFilePath = serializedObject.FindProperty("responseFilePath");
	}

	public override void OnInspectorGUI() 
    {
        // These big ugly block things are how to make a file path field. 
        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(p_animListPath);
        if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {

            // Adding this as a delegate prevents errors, because it waits 
            // until after the OnInspectorGUI method to finish running before
            // displaying the file dialog. 
            EditorApplication.delayCall += () => {
                string s = EditorUtility.OpenFilePanel(
                    "Select Animation List File",
                    "Assets/Resources",
                    "txt"
                );

                if (s != string.Empty) {
                    s = MoShUtilities.PathRelativeOrAbsolute(s);
                    p_animListPath.stringValue = s;
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }

        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(p_AnimFolder);
        if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {
            
            EditorApplication.delayCall += () => {
                string s = EditorUtility.OpenFolderPanel(
                    "Select Animation Folder",
                    "Assets/Resources",
                    p_AnimFolder.stringValue
                );

                if (s != string.Empty) {
                    s = MoShUtilities.PathRelativeOrAbsolute(s);
                    p_AnimFolder.stringValue = s;
                    serializedObject.ApplyModifiedProperties();
                }

            };
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(p_responseFilePath);
        if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {
            EditorApplication.delayCall += () => {
                string s = EditorUtility.SaveFilePanel(
                    "Save Responses As",
                    "Assets",
                    p_responseFilePath.stringValue,
                    "csv"
                );
                if (s != string.Empty) {
                    s = MoShUtilities.PathRelativeOrAbsolute(s);
                    p_responseFilePath.stringValue = s;
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();

        DrawDefaultInspector();
    }


}
