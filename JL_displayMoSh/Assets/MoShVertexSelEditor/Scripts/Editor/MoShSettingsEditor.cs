// Joseph Landy <14jnl@queensu.ca>
// 2018-03-26


using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using SimpleJSON;
using UnityEditor.SceneManagement;

namespace MoShVertexSelectionBuilder {
    [CustomEditor(typeof(MoShSettingsGen))]
    public class MoShSettingsEditor : Editor {

        public static string[] SearchPathRoot = { "Assets/MoShVertexSelEditor" };

        MoShSettingsGen settings;
        VertexSelector selector;

        Vector3[] vertices;
        //	[SerializeField]
        //	List<Vector3> vertices;


        // I think if this is moved out of the editor into the settings object it will persist after deselection,
        // even though dictionaries aren't serializable. 
        Dictionary<int, int> mapping;

        //Use the ID generator object to produce a unique ID for each marker. 
        [SerializeField]
        IDGenerator IDs;


        [MenuItem("MoSh/Vertex Selection Editor")]
        public static void EditMoshSettings()
        {
            EditorSceneManager.OpenScene("Assets/MoShVertexSelEditor/moshsettings_scene.unity", OpenSceneMode.Single);
        }


        void Awake()
        {
            settings = target as MoShSettingsGen;
            selector = settings.GetComponent<VertexSelector>();

            // collect and store an array of vertices. 
            vertices = settings.GetComponent<MeshFilter>().sharedMesh.vertices;
            //		Mesh sm = settings.GetComponent<MeshFilter> ().sharedMesh;

            //		vertices = new List<Vector3> (sm.vertexCount);

            IDs = AssetDatabase.LoadAssetAtPath<IDGenerator>("Assets/MoShVertexSelEditor/MarkerIDGenerator.asset");
            FindDuplicates();
        }


        public override void OnInspectorGUI()
        {

            GUILayout.Label("Note: this tab must be open to access Scene View functionality");
            if (settings.SettingsLoaded) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Male"))
                    setGender(Genders.MALE);
                if (GUILayout.Button("Female"))
                    setGender(Genders.FEMALE);

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Selection From Marker Definition File. ")) {
                string fp = EditorUtility.OpenFilePanel("load settings", Environment.CurrentDirectory, "json");
                LoadSpecificSettings(fp);
                SceneView.RepaintAll(); // Have to call an immediate repaint. 
            }
            EditorGUILayout.BeginHorizontal();



            if (GUILayout.Button("New Selection")) {
                CreateNewSettings();
                SceneView.RepaintAll();
            }
            else if (GUILayout.Button("Load Sample")) {
                LoadBasicSettings();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            if (settings.SettingsLoaded) {
                if (GUILayout.Button("Save Selection To Disk"))
                    saveas();

                // List of markers etc are drawn according to the default interface as would be drawn automatically
                // if an editor script hadn't been defined.

                DrawDefaultInspector();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add vertex")) {
                    //				Repaint ();
                    //				GUI.FocusControl (null);
                    addMarker();
                    Repaint();
                    SceneView.RepaintAll();
                }
                else if (GUILayout.Button("Remove vertex")) {
                    settings.DeleteSelected();
                    // the editor updates to reflect this immediately, but the scene view doesn't.
                    SceneView.RepaintAll();
                }
                else if (GUILayout.Button("Change vertex")) {
                    if (selector.VertIndex.HasValue) {
                        bool success = assignVertexToMarker(selector.VertIndex.Value);
                        if (success)
                            SceneView.RepaintAll();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Returns true if v was successfully assigned to a marker. 
        /// </summary>
        bool assignVertexToMarker(int v)
        {
            if (settings.selection == null)
                return false;

            settings.selection.vertex = CorrectVertexIndex(v);
            return true;
        }


        /// <summary>
        /// Sets the gender, updating json data structure, and switching the meshes etc.
        /// </summary>
        /// <param name="g">new gender.</param>
        void setGender(Genders g)
        {
            //		// reassign everything that depends on the mesh. 
            //		if (SettingsLoaded && settings.gender == g) {
            //			return;
            //		}
            Mesh gmesh = loadGenderMesh(g);
            settings.AssignMesh(gmesh);
            // have to reassign a bunch of stuff in the VertexSelector.
            settings.GetComponent<VertexSelector>().initialize();
            settings.gender = g;
            settings.JsonTree["gender"] = g == Genders.MALE ? "male" : "female";
        }

        /// <summary>
        /// Loads the mesh for the specified gender.
        /// </summary>
        /// <returns>Mesh from template_male or template_female</returns>
        /// <param name="g">Gender to load.</param>
        static Mesh loadGenderMesh(Genders g)
        {


            string[] searchrslt = null;
            if (g == Genders.MALE) {
                searchrslt = AssetDatabase.FindAssets("template_male", SearchPathRoot);
            }
            else if (g == Genders.FEMALE) {
                searchrslt = AssetDatabase.FindAssets("template_female_softnormals", SearchPathRoot);
            }
            if (searchrslt == null) {
                Debug.LogError("Project must contain template mesh files.");
                return null;
            }
            string guid = searchrslt[0];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Mesh genderedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            return genderedMesh;
        }


        // I don't think this will work very well because I don't think HSV is properly linearized. Or delinearized.
        // I can honestly never remember which is which. I like colours, but only when other people have to deal with them!
        /// <summary>
        /// Evenly divides the continuous color spectrum into a sequence of
        /// <paramref name="numColours"/> discrete colours of equal intensity and saturation.
        /// Returns the colour in the sequence at <paramref name="index"/>
        /// </summary>
        /// 
        /// <returns>The color.</returns>
        /// <param name="index">Index.</param>
        /// <param name="numColours">Number of colours.</param>
        public static Color IndexColor(int index, int numColours)
        {
            float H = ((float)index) / numColours;
            return Color.HSVToRGB(H, 1f, 1f);
        }

        /// <summary>
        /// Load a sample json settings file.
        /// </summary>
        void LoadBasicSettings()
        {
            settings.JsonTree = LoadSampleJSON();

            settings.Markers = new List<Marker>();
            JSONArray markersets = settings.JsonTree["markersets"].AsArray;
            int markersetIndex = 0;
            foreach (JSONNode markerset in markersets) {
                JSONObject indices = markerset["indices"].AsObject;
                foreach (KeyValuePair<string, JSONNode> marker in indices) {
                    settings.Markers.Add(
                        new Marker(marker.Key, marker.Value.AsInt, markersetIndex, IDs.NextID)
                    );
                }
                markersetIndex++;
            }
            for (int i = 0; i < settings.Markers.Count; i++) {
                settings.Markers[i].displayColor = IndexColor(i, settings.Markers.Count);
            }
            if (settings.JsonTree["gender"] == "female") {
                setGender(Genders.FEMALE);
            }
            else {
                setGender(Genders.MALE);
            }
            settings.SettingsLoaded = true;
        }


        public static JSONNode MarkersTemplate()
        {
            JSONNode jstree = new JSONObject();
            jstree["gender"] = "male";
            jstree["markersets"] = new JSONArray();
            jstree["markersets"].Add(new JSONObject());
            jstree["markersets"][0]["distance_from_skin"] = 0.0095;
            jstree["markersets"][0]["indices"] = new JSONObject();
            jstree["markersets"][0]["marker_radius"] = 0.0095;
            jstree["markersets"][0]["template_fname"] = "";
            jstree["markersets"][0]["type"] = "ball";
            return jstree;
        }

        public static JSONNode VertexTemplate()
        {
            JSONNode jstree = new JSONObject();
            jstree["gender"] = "male";
            jstree["markersets"] = new JSONArray();
            jstree["markersets"].Add(new JSONObject());
            jstree["markersets"][0]["distance_from_skin"] = 0.0095;
            jstree["markersets"][0]["indices"] = new JSONObject();
            jstree["markersets"][0]["marker_radius"] = 0.0095;
            jstree["markersets"][0]["template_fname"] = "";
            jstree["markersets"][0]["type"] = "ball";
            return jstree;
        }

        void CreateNewSettings()
        {
            settings.JsonTree = MarkersTemplate();
            settings.Markers = new List<Marker>();
            settings.SettingsLoaded = true;
        }


        static JSONNode LoadSampleJSON()
        {
            string[] searchResults = AssetDatabase.FindAssets("settingsBase");
            if (searchResults == null || searchResults.Length != 1) {
                Debug.LogError("There must be one and only one file named settingsBase in the project.");
                return null;
            }
            string settingsPath = AssetDatabase.GUIDToAssetPath(searchResults[0]);
            TextAsset jsonsettings = AssetDatabase.LoadAssetAtPath(settingsPath, typeof(TextAsset)) as TextAsset;
            return JSONNode.Parse(jsonsettings.text);
        }


        static JSONNode LoadSpecificJSON(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return null;
            string jsonstring;
            using (StreamReader sr = new StreamReader(filepath)) {
                jsonstring = sr.ReadToEnd();
            }
            return JSONNode.Parse(jsonstring);
        }


        /// <summary>
        /// Load and display the marker assignments from the file at <paramref name="filepath"/>, to edit an existing
        /// assignment.
        /// </summary>
        /// <param name="filepath">File path.</param>
        void LoadSpecificSettings(string filepath)
        {
            settings.JsonTree = LoadSpecificJSON(filepath);
            if (settings.JsonTree == null) {
                return;
            }
            settings.Markers = new List<Marker>();
            JSONArray markersets = settings.JsonTree["markersets"].AsArray;
            int setIndex = 0;

            foreach (JSONNode markerset in markersets) {
                JSONObject indices = markerset["indices"].AsObject;

                foreach (KeyValuePair<string, JSONNode> marker in indices) {

                    settings.Markers.Add(
                        new Marker(
                            marker.Key,
                            CorrectVertexIndex(marker.Value.AsInt),
                            setIndex,
                            IDs.NextID
                        )
                    );
                }
                setIndex++;

            }

            for (int i = 0; i < settings.Markers.Count; i++) {
                settings.Markers[i].displayColor = IndexColor(i, settings.Markers.Count);
            }
            settings.SettingsLoaded = true;
        }


        void saveas()
        {
            string fp = EditorUtility.SaveFilePanel("save as", Environment.CurrentDirectory, "settings", "json");
            SaveSettingsAs(fp);
        }


        /// <summary>
        /// Saves the loaded json data to a file. 
        /// </summary>
        /// <param name="filepath">File path.</param>
        void SaveSettingsAs(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) {
                return;
            }
            updateInTree();
            // OK, so to get this to just be a simple vertex selection, I'm not going to change anything here,
            // I'm just going to make a new JSON file by flattening the one for this. 
            // replacing:
            //string jsonstring = settings.JsonTree.ToString(4);
            //File.WriteAllText(filepath, jsonstring);
            // with:

            //JSONNode jstree = new JSONObject();
            //jstree["gender"] = "male";
            //jstree["markersets"] = new JSONArray();
            //jstree["markersets"].Add(new JSONObject());
            //jstree["markersets"][0]["distance_from_skin"] = 0.0095;
            //jstree["markersets"][0]["indices"] = new JSONObject();
            //jstree["markersets"][0]["marker_radius"] = 0.0095;
            //jstree["markersets"][0]["template_fname"] = "";
            //jstree["markersets"][0]["type"] = "ball";

            JSONNode selected = new JSONObject();
                        
            foreach (JSONNode markerset in settings.JsonTree["markersets"].AsArray.Children) {
                foreach (KeyValuePair<string, JSONNode> vertex in markerset["indices"]) {
                    // vertex will be a key value pair. 
                    selected[vertex.Key] = vertex.Value.AsInt;
                }
            }

            string selstring = selected.ToString(4);
            File.WriteAllText(filepath, selstring);
        }

        /// <summary>
        /// Marker positions are stored in an array for editing. This method updates the JSON data structure
        /// with the values from the array Markers. Must be done before saving the JSON file. 
        /// </summary>
        void updateInTree()
        {
            foreach (JSONNode markerset in settings.JsonTree["markersets"]) {
                markerset["indices"] = new JSONObject();
            }
            foreach (Marker marker in settings.Markers) {
                // modified to support a set of selections without labels entered.
                if (string.IsNullOrEmpty(marker.label)) {
                    marker.label = Convert.ToString(marker.vertex);
                }
                settings.JsonTree["markersets"][0]["indices"][marker.label] = marker.vertex;
            }

        }



        //add a Marker to the list, and return a reference to it. 
        Marker addMarker()
        {
            foreach (Marker marker in settings.Markers) {
                marker.justCreated = false;
            }
            Marker newMarker;

            if (selector.HasSelection)
                newMarker = new Marker(
                    "",
                    CorrectVertexIndex(selector.VertIndex.Value),
                    0,
                    IDs.NextID,
                    true);
            else
                newMarker = new Marker("", -999, 0, IDs.NextID, true);
            settings.Markers.Add(newMarker);
            //		settings.OnSelected (newMarker); //make the marker start as selected.
            settings.MarkerSelected(newMarker.uniqueID);
            return newMarker;
        }


        int CorrectVertexIndex(int index)
        {
            return index >= 6890 ? mapping[index] : index;
        }


        public void FindDuplicates()
        {
            // I'm taking a precomputation approach. I will have to test if this works on windows as well. 
            // it's possible vertex ordering is applied differently with directX. It does seem unlikely though. 
            // and with later versions of unity. Mesh stuff may have been changed with Unity 17.3, since they
            // added support for 32 bit mesh buffers, over 16 bits (current limit).
            // if the order is always the same, then it will be fine. 
            // Dictionary<int, int> mapping = new Dictionary<int, int> ();
            int duplicateCount = 0;
            mapping = new Dictionary<int, int>();

            for (int i = 6890; i < vertices.Length; i++) {

                //int rslt = vertices (v => v == vertices [i]);
                int rslt = Array.FindIndex<Vector3>(vertices, v => v == vertices[i]);

                mapping[i] = rslt;

                //			Debug.Log ("searching for vertex " + i + " result: " + rslt);
                Debug.Assert(rslt != -1, "No match found! (this is impossible)");
                Debug.Assert(rslt <= 6889, "match found greater than 6889!");
                Debug.Assert(rslt != i, "no duplicate vertex for vertex at index: " + i);

                if (rslt != -1 && rslt <= 6889) {
                    duplicateCount++;
                }
            }
            Debug.Assert(duplicateCount == vertices.Length - 6890, "some vertices over 6889 had no duplicate");
        }
    }
}
