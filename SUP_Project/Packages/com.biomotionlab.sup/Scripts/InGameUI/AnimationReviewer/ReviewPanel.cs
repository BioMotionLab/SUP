using System.Collections.Generic;
using Playback;
using TMPro;
using UnityEngine;

namespace InGameUI.AnimationReviewer {
    public class ReviewPanel : MonoBehaviour {

        [SerializeField]
        TextMeshProUGUI filePathDisplay = default;
    
        string reviewFilePath;
    
        public List<MoshAnimation> currentAnims;
        public List<MoshAnimation> CurrentAnims => currentAnims;
    
        public string ReviewFilePath => reviewFilePath;
    

        void OnEnable() {
            PlaybackEventSystem.OnPlayingNewAnimationSet += AnimationSetChanged;
        }
    
        void OnDisable() {
            PlaybackEventSystem.OnPlayingNewAnimationSet -= AnimationSetChanged;
        }

        void AnimationSetChanged(List<MoshAnimation> animations) {
            currentAnims = animations;
        }

    
        public void FileSelected(string[] files) {
            if (files.Length == 0) return;
            FileSelected(files[0]);
        }

        void UpdateFilePathDisplay() {
            filePathDisplay.text = $"Review File: {reviewFilePath}";
        }

        public void FileSelected(string file) {
            reviewFilePath = file.Replace("\\", "\\\\");
            Debug.Log(reviewFilePath);
            UpdateFilePathDisplay();
        }
    }
}
