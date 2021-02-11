using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.Scripts.SMPLModel;
using Playback;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MoshPlayer.AnimationReviewer {
    public class SaveNoteButton : MonoBehaviour {

        [SerializeField]
        ReviewPanel reviewPanel = default;

        [SerializeField]
        TMP_InputField noteText = default;

        [SerializeField] float noteClearTime = 1.5f;
        
        List<NoteChangedFlag> noteChangedFlags = new List<NoteChangedFlag>();
        
        void OnEnable() {
            noteText.onValueChanged.AddListener(NoteChanged);
        }

        void OnDisable() {
            noteText.onValueChanged.RemoveListener(NoteChanged);
        }
        

        [PublicAPI]
        public void SaveNoteToFile() {
            string note = noteText.text;
            if (IsSpecialText(note)) DebugBetas();
            
            string animationStrings = "";
            foreach (AMASSAnimation animation in reviewPanel.currentAnims) {
                animationStrings += animation.AnimationName + " ";
            }

            string newLine = animationStrings + "," + note + "\n";

            if (!File.Exists(reviewPanel.ReviewFilePath)) {
                StartCoroutine(DisplayError("NO REVIEW FILE FOUND"));
                return;
            }

            
            if (string.IsNullOrWhiteSpace(note)) { ;
                StartCoroutine(DisplayError("NO NOTE!"));
                return;
            }

            if (string.IsNullOrWhiteSpace(animationStrings)) {
                Debug.LogWarning("Review pane could not gather current anims");
                StartCoroutine(DisplayError("NO CURRENT ANIMATIONS"));
                return;
            }
            Debug.Log($"Saving note: {note}");
            File.AppendAllText(reviewPanel.ReviewFilePath, newLine);
            noteText.SetTextWithoutNotify("");
            StartCoroutine(DisplayError("SAVED."));
        }

        IEnumerator DisplayError(string errorText) {
            NoteChangedFlag changedFlag = new NoteChangedFlag();
            noteChangedFlags.Add(changedFlag);
            string currentText = noteText.text;
            float startTime = Time.time;
            noteText.SetTextWithoutNotify(errorText);
            while (Time.time - startTime < noteClearTime) {
                yield return null;
            }
            if (!changedFlag.Changed) noteText.SetTextWithoutNotify(currentText);
            noteChangedFlags.Remove(changedFlag);
        }

        void NoteChanged(string unused) {
            foreach (NoteChangedFlag noteChangedFlag in noteChangedFlags) {
                noteChangedFlag.Flip();
            }
        }

        class NoteChangedFlag {
           
            public bool Changed = false;
            public void Flip() {
                Changed = true;
            }
        }

        void DebugBetas() {
            IndividualizedBody[] foundObjects = FindObjectsOfType<IndividualizedBody>();
            foreach (IndividualizedBody body in foundObjects) {
                body.SetDebugBetas();
            }
        }

        bool IsSpecialText(string note) {
            if (String.Equals(note, "baby", StringComparison.CurrentCultureIgnoreCase)) return true;
            return false;
        }
    }
}
