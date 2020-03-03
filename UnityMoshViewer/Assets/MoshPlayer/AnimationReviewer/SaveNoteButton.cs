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


    string currentAnims;

    void OnEnable() {
        PlaybackEventSystem.OnPlayingNewAnimationSet += AnimationSetChanged;
    }
    
    void OnDisable() {
        PlaybackEventSystem.OnPlayingNewAnimationSet -= AnimationSetChanged;
    }

    void AnimationSetChanged(string animations) {
        currentAnims = animations;
    }

    
    [PublicAPI]
    public void SaveNoteToFile() {
        string note = noteText.text;
        Debug.Log(note);
        if (string.IsNullOrWhiteSpace(note)) return;
        
        if (File.Exists(ReviewPanel.ReviewFilePath)) {
            string newLine = currentAnims + "," + note + "\n";
            File.AppendAllText(ReviewPanel.ReviewFilePath, newLine);
        }
        else {
            noteText.text = "NO REVIEW FILE FOUND";
        }

    }
}
