using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(AnimReviewer))]
public class AnimReviewerEditor : Editor {

    SerializedProperty animListPath;
    SerializedProperty animFolder;
    SerializedProperty responseFilePath;

	void OnEnable() {
        animListPath = serializedObject.FindProperty(nameof(AnimReviewer.AnimListPath));
        animFolder = serializedObject.FindProperty(nameof(AnimReviewer.AnimFolder));
	}

	public override void OnInspectorGUI() 
    {
        ShowAnimListPath();
        ShowAnimFolder();

        serializedObject.ApplyModifiedProperties();
        DrawDefaultInspector();
    }

    void ShowAnimListPath() {
        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(animListPath);
        if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {
            // Adding this as a delegate prevents errors, because it waits 
            // until after the OnInspectorGUI method to finish running before
            // displaying the file dialog. 
            EditorApplication.delayCall += () => {
                string selectedPath = EditorUtility.OpenFilePanel(
                                                       "Select Animation List File",
                                                       "Assets/Resources",
                                                       "txt"
                                                      );
                Debug.LogWarning(selectedPath);
                
                if (selectedPath != string.Empty) {
                    //selectedPath = PathRelativeOrAbsolute(selectedPath);
                    animListPath.stringValue = selectedPath;
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }

        GUILayout.EndHorizontal();
    }

    void ShowAnimFolder() {
        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(animFolder);
        if (GUILayout.Button("Select...", GUILayout.ExpandWidth(false))) {
            EditorApplication.delayCall += () => {
                string selectedPath = EditorUtility.OpenFolderPanel(
                                                         "Select Animation Folder",
                                                         "Assets/Resources",
                                                         animFolder.stringValue
                                                        );

                if (selectedPath != string.Empty) {
                    //selectedPath = PathRelativeOrAbsolute(selectedPath);
                    animFolder.stringValue = selectedPath;
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }

        GUILayout.EndHorizontal();
    }
    
}
