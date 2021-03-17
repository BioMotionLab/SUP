using System.IO;
using JetBrains.Annotations;
using Playback;
using SFB;
using TMPro;
using UnityEngine;

namespace InGameUI {
    public class SelectAnimationsPanel : MonoBehaviour {

        [SerializeField] TextMeshProUGUI folderText = default;

        [SerializeField] TextMeshProUGUI singleFileText = default;
        
        [SerializeField] TextMeshProUGUI fileText = default;

        [SerializeField] TextMeshProUGUI singleErrorText = default;
        
        [SerializeField] TextMeshProUGUI errorText = default;

        
        
        string animationsFolder;
        string listFile;

        bool folderSelected = false;
        bool listSelected   = false;
        string singleFile;
        bool singleFileSelected = false;
        ExtensionFilter supportedExtensions;

        void Start() {
            supportedExtensions = new ExtensionFilter {Extensions = new[] {"json", "h5"}};
        }

        [PublicAPI]
        public void SelectFolder() {
            StandaloneFileBrowser.OpenFolderPanelAsync("Select Folder", "", false, SelectedFolder);
        }

        void SelectedFolder(string[] paths) {
            if (paths.Length == 0) return;
            animationsFolder = paths[0].Replace("\\", "\\\\");
            Debug.Log(animationsFolder);
            folderText.text = animationsFolder;
            folderSelected = true;
        }

        [PublicAPI]
        public void SelectListFile() {
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", "", false, SelectedListFile);
        }

        void SelectedListFile(string[] files) {
            listFile = files[0].Replace("\\", "\\\\");
            Debug.Log(listFile);
            fileText.text = listFile;
            listSelected = true;
        }

        [PublicAPI]
        public void SelectSingleFile() {
            StandaloneFileBrowser.OpenFilePanelAsync("Open File", "", new[] {supportedExtensions}, false, SelectedSingleFile);
        }

        void SelectedSingleFile(string[] files) {
            if (files.Length < 1) return;
            singleFile = files[0].Replace("\\", "\\\\");
            Debug.Log(singleFile);
            singleFileText.text = Path.GetFileName(singleFile);
            singleErrorText.text = "";
            singleFileSelected = true;
        }

        [PublicAPI]
        public void LoadAnimations() {
            if (!folderSelected || !listSelected) {
                errorText.text = "Missing list files or animation folder!";
                return;
            }
            PlaybackEventSystem.LoadAnimations(listFile, animationsFolder);
            gameObject.SetActive(false);
        }
        
        
        [PublicAPI]
        public void LoadSingleAnimation() {
            if (!singleFileSelected) {
                singleErrorText.text = "Missing single files";
                return;
            }
            PlaybackEventSystem.LoadSingleAnimation(singleFile);
            gameObject.SetActive(false);
        }
    
        [PublicAPI]
        public void LoadSamples() {
            animationsFolder = Application.streamingAssetsPath + "//SampleAnimations";
            listFile = Application.streamingAssetsPath + "//SampleAnimationList.txt";
            PlaybackEventSystem.LoadAnimations(listFile, animationsFolder);
            gameObject.SetActive(false);
        }
    
    
    }
}
