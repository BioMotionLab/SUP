using System.IO;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace MoshPlayer.AnimationReviewer {
    public class SaveNoteButton : MonoBehaviour {

        [SerializeField]
        ReviewPanel reviewPanel = default;

        [SerializeField]
        TMP_InputField noteText = default;
    
    
        [PublicAPI]
        public void SaveNoteToFile() {
            string note = noteText.text;
            string newLine = reviewPanel.CurrentAnims + "," + note + "\n";
        
            Debug.Log(note);
            if (string.IsNullOrWhiteSpace(note)) return;
        
            if (string.IsNullOrWhiteSpace(reviewPanel.CurrentAnims)) Debug.LogWarning("Review pane could not gather current anims");
        
            if (File.Exists(reviewPanel.ReviewFilePath)) {
            
                File.AppendAllText(reviewPanel.ReviewFilePath, newLine);
            }
            else {
                noteText.text = "NO REVIEW FILE FOUND";
            }

        }
    }
}
