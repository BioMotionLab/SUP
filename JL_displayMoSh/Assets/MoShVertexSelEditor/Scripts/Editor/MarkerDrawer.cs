// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26

using UnityEngine;
using UnityEditor;

namespace MoShVertexSelectionBuilder {
    [CustomPropertyDrawer(typeof(Marker))]
    public class MarkerDrawer : PropertyDrawer {
        //	VertexSelector vs2 = Selection.activeGameObject.GetComponent<VertexSelector> ();


        public override void OnGUI(Rect position, SerializedProperty data, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, data);

            if (data.FindPropertyRelative("IsSelected").boolValue) {
                EditorGUI.DrawRect(position, Color.blue);
            }


            Rect labelRect = new Rect(position);
            labelRect.width *= 0.2f;

            // makes newly created markers have the label field in focus for editing. 
            //		if (data.FindPropertyRelative ("justCreated").boolValue) {
            //			Debug.Log ();
            //			data.FindPropertyRelative ("justCreated").boolValue = false;
            //			GUI.SetNextControlName ("focus marker");
            //			EditorGUI.PropertyField (labelRect, data.FindPropertyRelative ("label"), GUIContent.none);
            //
            //			GUI.FocusControl ("focus marker");
            //			EditorGUI.FocusTextInControl ("focus marker");
            //			data.FindPropertyRelative ("justCreated").boolValue = false;
            //		} else {
            //			GUI.SetNextControlName (null);
            //			EditorGUI.PropertyField (labelRect, data.FindPropertyRelative ("label"), GUIContent.none);
            //		}
            EditorGUI.PropertyField(labelRect, data.FindPropertyRelative("label"), GUIContent.none);

            Rect remaining = new Rect(position);
            remaining.x = labelRect.xMax;
            Rect vertexRect = new Rect(remaining);

            vertexRect.width = 75f; // a number can definitely have a fixed width. 

            remaining.x = vertexRect.xMax + 10f;
            //		var colorRect = new Rect (remaining);
            //		colorRect.width = 50f;
            //
            EditorGUI.PropertyField(vertexRect, data.FindPropertyRelative("vertex"), GUIContent.none);
            //		EditorGUI.PropertyField (colorRect, data.FindPropertyRelative ("displayColor"), GUIContent.none);
            //
            //		remaining.x = colorRect.xMax;
            Rect setButtonRect = new Rect(remaining);
            setButtonRect.width = 80f;

            //		if (GUI.Button (setButtonRect, "Mark Vertex")) {
            //			VertexSelector vs = Selection.activeGameObject.GetComponent<VertexSelector> ();
            //			if (vs != null) {
            //				int? vertex = vs.VertIndex;
            //				if (vertex.HasValue) {
            //					data.FindPropertyRelative ("vertex").intValue = vertex.Value;
            //				}	
            //			}
            //		}
            //		remaining.x = setButtonRect.xMax;
            Rect selectBtnRect = new Rect(remaining);
            selectBtnRect.width = 100f;

            if (GUI.Button(selectBtnRect, "Select")) {
                // The delegate way isn't going to work. But, I can do something else. 
                MoShSettingsGen sg = Selection.activeGameObject.GetComponent<MoShSettingsGen>();
                // I can call a public method on the MoShSettingsGen with the ID of this marker. 
                sg.MarkerSelected(data.FindPropertyRelative("uniqueID").intValue);
                SceneView.RepaintAll();
            }
            EditorGUI.EndProperty();
        }
    }
}
