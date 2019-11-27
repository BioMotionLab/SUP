using UnityEngine;
using System.IO;
using System;
using UnityEngine.Serialization;

/// <summary>
/// View a sequence of MoSh animations read from a file, and record right or left.
/// </summary>
public class AnimReviewer : MonoBehaviour {

    const string DefaultSelectPathText = "Select...";
	string[] animLines;

    public string[][] files;
	

    [FormerlySerializedAs("animListPath")]
    [Tooltip("Path to file with list of animation file names")]
    [HideInInspector]
    [SerializeField] 
    public string AnimListPath = DefaultSelectPathText;


	[Tooltip("Folder containing MoSh anims")]
    [HideInInspector]
    [SerializeField] 
	public string AnimFolder = DefaultSelectPathText;

	[FormerlySerializedAs("responseFilePath")]
    [Tooltip("Path to output file for saving responses")]
    [HideInInspector]
    [SerializeField] 
	public string ResponseFilePath = "Assets/responses.csv";

    [FormerlySerializedAs("overwrite")]
    [Tooltip("Overwrite or append to the response file.")]
    [SerializeField] 
    public bool Overwrite = true;
	
	[FormerlySerializedAs("AnimPlayer1")]
    [Tooltip("Drag objects with MoshCharacter components into these " +
             "fields. Use instances of the MoShViewer prefab.")]
    [SerializeField] 
    public MoshCharacter Character1;

    [FormerlySerializedAs("AnimPlayer2")]
    [Tooltip("Drag objects with MoshCharacter components into these " +
             "fields. Use instances of the MoShViewer prefab.")]
    [SerializeField] 
    public MoshCharacter Character2;


    int animIndex = 0;
	
	void Start () {
		// read the list of fileNames.
		animLines = File.ReadAllLines(AnimListPath);
        
        StartAnimation(animIndex); //play the first animation!
	}


	void Update () {
		
        // Controls to step through the animation. 
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightBracket)) {
            // skip to next animation.
            animIndex++;
            if (animIndex >= animLines.Length) {
                animIndex = animLines.Length - 1;
            }
            else {
                
                if (animIndex < 0 || animIndex >= animLines.Length) {
                    return; // if there are no animations left in the specified direction. 
                }
                StartAnimation(animIndex);
            }
        }
	}


	/// <summary>
	/// Play the animation for both characters at specified position in sequence of files.
	/// </summary>
	/// <param name="animationIndex"></param>
	void StartAnimation(int animationIndex) 
    {
		string[] animationFileStrings = GetAnimationFiles(animationIndex);
		
		Character1.StartAnimation(animationFileStrings[0]);
		Character2.StartAnimation(animationFileStrings[1]);
		
	}

	string[] GetAnimationFiles(int animationIndex) {
		//TODO maybe better way to store list of animations? Needs to be MatLab-friendly for Niko.
		string[] fileNames = animLines[animationIndex].Split (' '); //Space delimited
		string[] animationFileStrings = new string[fileNames.Length];
		for (int index = 0; index < fileNames.Length; index++) {
			string filename = fileNames[index];
			animationFileStrings[index] =  LoadAnimFileAsString(filename);
		}
		return animationFileStrings;
	}

	string LoadAnimFileAsString(string filename) {
		string animFilePath1 = Path.Combine(AnimFolder, filename);
        string animText1 = File.ReadAllText(animFilePath1);
        return animText1;
    }



 
    
}