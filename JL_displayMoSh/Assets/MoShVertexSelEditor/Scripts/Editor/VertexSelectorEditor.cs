// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26

using UnityEngine;
using UnityEditor;

namespace MoShVertexSelectionBuilder {
    /// <summary>
    /// Custom editor for a vertex selector component. 
    /// </summary>
    [CustomEditor(typeof(VertexSelector))]
    public class VertexSelectorEditor : Editor {

        VertexSelector selector;


        void OnEnable()
        {
            selector = target as VertexSelector;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Note: this tab must be open to access functionality in Scene View.");
            DrawDefaultInspector();
            if (selector.VertIndex.HasValue) {
                GUILayout.Label("Selection:\t" + selector.VertIndex.ToString());
            }
            else {
                GUILayout.Label("Selection:\tnone");
            }
            if (GUILayout.Button("Clear Selection")) {
                selector.ClearSelection();
                // This class seems to be undocumented. That could mean the specification is unstable, and 
                // it's behavior may change over time. I wanted to repaint the SceneView though, and this 
                // looks like it will repaint the SceneView. 
                SceneView.RepaintAll();
            }
        }


        void OnSceneGUI()
        {
            //Note: I'm pretty sure there are multiple ways of doing the raycasting. 
            Event e = Event.current;

            if (e.isMouse && e.button == 0 && e.type == EventType.MouseDown) { //0 is left button
                                                                               // is e.mousePosition in gui space (GUIPoint) or screen space? 
                Ray r = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                object possiblehit = HandleUtility.RaySnap(r);
                if (possiblehit != null) {
                    selector.TargetHit((RaycastHit)possiblehit);
                    Repaint(); // update the inspector with the new selected vertex. 
                }
            }
        }


    }
}
