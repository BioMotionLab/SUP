using MoshPlayer.Scripts.Playback;
using TMPro;
using UnityEngine;

namespace MoshPlayer.AnimationReviewer {
    public class ReviewPanel : MonoBehaviour {

        [SerializeField]
        TextMeshProUGUI filePathDisplay = default;
    
        string reviewFilePath;
    
        public string currentAnims;
        public string CurrentAnims => currentAnims;
    
        public string ReviewFilePath => reviewFilePath;
    

        void OnEnable() {
            PlaybackEventSystem.OnPlayingNewAnimationSet += AnimationSetChanged;
        }
    
        void OnDisable() {
            PlaybackEventSystem.OnPlayingNewAnimationSet -= AnimationSetChanged;
        }

        void AnimationSetChanged(string animations) {
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
