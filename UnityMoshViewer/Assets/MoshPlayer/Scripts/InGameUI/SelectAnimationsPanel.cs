using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.ThirdParty.StandaloneFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.InGameUI {
    public class SelectAnimationsPanel : MonoBehaviour {

        [FormerlySerializedAs("FolderText")] [SerializeField]
        TextMeshProUGUI folderText = default;

        [SerializeField]
        TextMeshProUGUI singleFileText = default;
        
        [FormerlySerializedAs("FileText")] [SerializeField]
        TextMeshProUGUI fileText = default;

        [SerializeField]
        TextMeshProUGUI singleErrorText = default;
        
        [FormerlySerializedAs("ErrorText")] [SerializeField]
        TextMeshProUGUI errorText = default;

        string animationsFolder;
        string listFile;

        bool folderSelected = false;
        bool listSelected   = false;
        string singleFile;
        bool singleFileSelected = false;
        


        [PublicAPI]
        public void SelectFolder() {
            var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
            if (paths.Length == 0) return;
            animationsFolder = paths[0].Replace("\\", "\\\\");
            Debug.Log(animationsFolder);
            folderText.text = animationsFolder;
            folderSelected = true;
        }

        [PublicAPI]
        public void SelectListFile() {
            string[] file = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
            listFile = file[0].Replace("\\", "\\\\");
            Debug.Log(listFile);
            fileText.text = listFile;
            listSelected = true;
        }

        [PublicAPI]
        public void SelectSingleFile() {
            string[] file = StandaloneFileBrowser.OpenFilePanel("Open File", "", "json", false);
            if (file.Length < 1) return;
            singleFile = file[0].Replace("\\", "\\\\");
            Debug.Log(singleFile);
            singleFileText.text = singleFile;
            singleErrorText.text = "";
            singleFileSelected = true;
        }

        [PublicAPI]
        public void LoadAnimations() {
            if (!folderSelected || !listSelected) {
                errorText.text = "Missing list file or animation folder!";
                return;
            }
            PlaybackEventSystem.LoadAnimations(listFile, animationsFolder);
            gameObject.SetActive(false);
        }
        
        
        [PublicAPI]
        public void LoadSingleAnimation() {
            if (!singleFileSelected) {
                singleErrorText.text = "Missing single file";
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
