using ThirdParty.StandaloneFileBrowser;
using UnityEngine;
// ReSharper disable All

namespace MoshPlayer.ThirdParty.StandaloneFileBrowser.Sample {
    public class BasicSample : MonoBehaviour {
        private string _path;

        void OnGUI() {
            var guiScale = new Vector3(Screen.width / 800.0f, Screen.height / 600.0f, 1.0f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, guiScale);

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            // Open File Samples

            if (GUILayout.Button("Open File")) {
                WriteResult(global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false));
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Open File Async")) {
                global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "", false, (string[] paths) => { WriteResult(paths); });
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Open File Multiple")) {
                WriteResult(global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanel("Open File", "", "", true));
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Open File Extension")) {
                WriteResult(global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanel("Open File", "", "txt", true));
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Open File Directory")) {
                WriteResult(global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanel("Open File", Application.dataPath, "", true));
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Open File Filter")) {
                var extensions = new [] {
                                            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
                                            new ExtensionFilter("Sound Files", "mp3", "wav" ),
                                            new ExtensionFilter("All Files", "*" ),
                                        };
                WriteResult(global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true));
            }

            GUILayout.Space(15);

            // Open Folder Samples

            if (GUILayout.Button("Open Folder")) {
                var paths = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", true);
                WriteResult(paths);
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Open Folder Async")) {
                global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFolderPanelAsync("Select Folder", "", true, (string[] paths) => { WriteResult(paths); });
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Open Folder Directory")) {
                var paths = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.OpenFolderPanel("Select Folder", Application.dataPath, true);
                WriteResult(paths);
            }

            GUILayout.Space(15);

            // Save File Samples

            if (GUILayout.Button("Save File")) {
                _path = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanel("Save File", "", "", "");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Save File Async")) {
                global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanelAsync("Save File", "", "", "", (string path) => { WriteResult(path); });
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Save File Default Name")) {
                _path = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", "");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Save File Default Name Ext")) {
                _path = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", "dat");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Save File Directory")) {
                _path = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanel("Save File", Application.dataPath, "", "");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Save File Filter")) {
                // Multiple save extension filters with more than one extension support.
                var extensionList = new [] {
                                               new ExtensionFilter("Binary", "bin"),
                                               new ExtensionFilter("Text", "txt"),
                                           };
                _path = global::ThirdParty.StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanel("Save File", "", "MySaveFile", extensionList);
            }

            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.Label(_path);
            GUILayout.EndHorizontal();
        }

        public void WriteResult(string[] paths) {
            if (paths.Length == 0) {
                return;
            }

            _path = "";
            foreach (var p in paths) {
                _path += p + "\n";
            }
        }

        public void WriteResult(string path) {
            _path = path;
        }
    }
}
