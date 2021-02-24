using System.IO;
using JetBrains.Annotations;
using ThirdParty.StandaloneFileBrowser;
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
            string[] file = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
            reviewPanel.FileSelected(file);
        }

        public void CreateFile() {
            string file = StandaloneFileBrowser.SaveFilePanel("Create New File", "", "review", "txt");
        
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
