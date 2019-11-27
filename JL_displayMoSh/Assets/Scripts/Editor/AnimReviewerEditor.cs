﻿using System.Collections;
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
        responseFilePath = serializedObject.FindProperty(nameof(AnimReviewer.ResponseFilePath));
	}

	public override void OnInspectorGUI() 
    {
        ShowAnimListPath();
        ShowAnimFolder();

        serializedObject.ApplyModifiedProperties();
        DrawDefaultInspector();
    }

    void ShowAnimListPath() {
        // These big ugly block things are how to make a file path field. 

        
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


    /// <summary>
    /// Check if a path is relative to the project, or an absolute path. 
    /// If it can be made relative, it's made relative. Otherwise the absolute
    /// path is returned. 
    /// </summary>
    /// <returns>The relative or absolute.</returns>
    /// <param name="path">Path.</param>
    static string PathRelativeOrAbsolute(string path)
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
