using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using MoshPlayer.ThirdParty.StandaloneFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.InGameUI {
    public class SelectAnimationsPanel : MonoBehaviour {

        [FormerlySerializedAs("folderText")]
        [SerializeField]
        TextMeshProUGUI FolderText = default;

        [FormerlySerializedAs("fileText")]
        [SerializeField]
        TextMeshProUGUI FileText = default;

        [FormerlySerializedAs("errorText")]
        [SerializeField]
        TextMeshProUGUI ErrorText = default;

        string animationsFolder;
        string listFile;

        bool folderSelected = false;
        bool listSelected   = false;
    
        [PublicAPI]
        public void SelectFolder() {
            var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
            if (paths.Length == 0) return;
            animationsFolder = paths[0].Replace("\\", "\\\\");
            Debug.Log(animationsFolder);
            FolderText.text = animationsFolder;
            folderSelected = true;
        }

        [PublicAPI]
        public void SelectFile() {
            string[] file = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
            listFile = file[0].Replace("\\", "\\\\");
            Debug.Log(listFile);
            FileText.text = listFile;
            listSelected = true;
        }

        [PublicAPI]
        public void LoadAnimations() {
            if (!folderSelected || !listSelected) {
                ErrorText.text = "Missing list file or animation folder!";
                return;
            }
            PlaybackEventSystem.LoadAnimations(listFile, animationsFolder);
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
