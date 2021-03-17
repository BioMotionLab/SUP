using System.IO;
using JetBrains.Annotations;
using SFB;
using UnityEngine;

namespace InGameUI.AnimationReviewer {
    public class ChangeFileButton : MonoBehaviour
    {
        ReviewPanel reviewPanel;

        void Awake() {
            reviewPanel = GetComponentInParent<ReviewPanel>();
        }


        [PublicAPI]
        public void SelectFile() {
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "", false, SelectedFile);
        }

        void SelectedFile(string[] file) {
            reviewPanel.FileSelected(file);
        }

        public void CreateFile() {
            StandaloneFileBrowser.SaveFilePanelAsync("Create New File", "", "review", "txt", CreatedFile);
        }

        void CreatedFile(string file) {
            
            if (string.IsNullOrEmpty(file)) {
                Debug.LogWarning("empty file given");
                return;
            }
        
            using(StreamWriter sw = File.AppendText(file))
            {
                sw.WriteLine("Animations, Note");
            }
            reviewPanel.FileSelected(file);
        }
    }
}
