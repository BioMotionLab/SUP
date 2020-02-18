using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MathNet.Numerics.LinearAlgebra;
using SFB;
using TMPro;
using UnityEngine;

public class SelectAnimationsPanel : MonoBehaviour {

    [SerializeField]
    TextMeshProUGUI folderText = default;

    [SerializeField]
    TextMeshProUGUI fileText = default;

    [SerializeField]
    TextMeshProUGUI errorText = default;

    string animationsFolder;
    string listFile;

    bool folderSelected = false;
    bool listSelected = false;
    
    [PublicAPI]
    public void SelectFolder() {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", false);
        animationsFolder = paths[0].Replace("\\", "\\\\");
        Debug.Log(animationsFolder);
        folderText.text = animationsFolder;
        folderSelected = true;
    }

    [PublicAPI]
    public void SelectFile() {
        string[] file = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        listFile = file[0].Replace("\\", "\\\\");
        Debug.Log(listFile);
        fileText.text = listFile;
        listSelected = true;
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
    
    
}
