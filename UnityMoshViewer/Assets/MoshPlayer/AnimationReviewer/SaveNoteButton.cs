using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using TMPro;
using UnityEngine;

public class SaveNoteButton : MonoBehaviour {

    [SerializeField]
    ReviewPanel ReviewPanel = default;

    [SerializeField]
    TMP_InputField noteText = default;
    
    
    [PublicAPI]
    public void SaveNoteToFile() {
        string note = noteText.text;
        string newLine = ReviewPanel.CurrentAnims + "," + note + "\n";
        
        Debug.Log(note);
        if (string.IsNullOrWhiteSpace(note)) return;
        
        if (string.IsNullOrWhiteSpace(ReviewPanel.CurrentAnims)) Debug.LogWarning("Review pane could not gather current anims");
        
        if (File.Exists(ReviewPanel.ReviewFilePath)) {
            
            File.AppendAllText(ReviewPanel.ReviewFilePath, newLine);
        }
        else {
            noteText.text = "NO REVIEW FILE FOUND";
        }

    }
}
