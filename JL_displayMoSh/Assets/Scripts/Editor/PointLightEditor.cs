using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PointLightDisplay))]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class PointLightEditor : Editor {
    PointLightDisplay pld;

    void OnEnable()
    {
        pld = target as PointLightDisplay;
        if (pld.PLPrefab == null) {
            // don't want to use serialized asset for this, I don't want it to have an undo step. 
            pld.PLPrefab = AssetDatabase.LoadAssetAtPath<Transform>("Assets/PointLightDisplay/PLDmarker.prefab");
        }
    }

    public override void OnInspectorGUI() {

        if (!pld.pldInstantiated) {
            if (GUILayout.Button("instantiate point lights.")) {
                EditorInstantiatePointLights();
            }
        }

        if (pld.pldInstantiated) {
            if (GUILayout.Button("remove point lights")) {
                EditorRemovePointLights();
            }
        }

        DrawDefaultInspector();
    }

    void EditorInstantiatePointLights()
    {
        pld.InstantiatePointLights();
    }

    // This can only be used in the editor, because DestroyImmediate shouldn't be
    // used in play mode. 
    void EditorRemovePointLights()
    {
        var smr = pld.GetComponent<SkinnedMeshRenderer>();
        smr.enabled = true;
        Transform root = smr.rootBone;
        MoShUtilities.EditorRemoveFromHierarchyByName(root, "PointLight");
        pld.pldInstantiated = false;
    }


}
