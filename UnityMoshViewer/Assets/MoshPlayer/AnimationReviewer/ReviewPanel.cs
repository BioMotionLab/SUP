using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReviewPanel : MonoBehaviour {

    [SerializeField]
    TextMeshProUGUI FilePathDisplay = default;
    
    string reviewFilePath;

    public string ReviewFilePath => reviewFilePath;

    public void FileSelected(string[] files) {
        if (files.Length == 0) return;
        FileSelected(files[0]);
    }

    void UpdateFilePathDisplay() {
        FilePathDisplay.text = $"Review File: {reviewFilePath}";
    }

    public void FileSelected(string file) {
        reviewFilePath = file.Replace("\\", "\\\\");
        Debug.Log(reviewFilePath);
        UpdateFilePathDisplay();
    }
}
