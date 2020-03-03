using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using MoshPlayer.ThirdParty.StandaloneFileBrowser;
using UnityEngine;

public class ChangeFileButton : MonoBehaviour
{
    ReviewPanel reviewPanel;

    void Awake() {
        reviewPanel = GetComponentInParent<ReviewPanel>();
    }


    [PublicAPI]
    public void SelectFile() {
        string[] file = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false);
        reviewPanel.FileSelected(file);
    }

    public void CreateFile() {
        string file = StandaloneFileBrowser.SaveFilePanel("Create New File", "", "review", "txt");
        using(StreamWriter sw = File.AppendText(file))
        {
            sw.WriteLine("Animations, Note");
        }
        reviewPanel.FileSelected(file);
    }
}
