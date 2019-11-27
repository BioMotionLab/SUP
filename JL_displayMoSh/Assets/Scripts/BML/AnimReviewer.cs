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
		if (AllAnimsComplete) return;
		// Controls to step through the animation. 
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightBracket)) {
            // skip to next animation.
            animIndex++;
			//TODO make this happen before increment. Problem is something is initialized in Start that breaks when played first here. Right now it starts at anim 1, after 0 starts during Start function.

			if (AllAnimsComplete) return;
			StartAnimation(animIndex);
		}
	}

	bool AllAnimsComplete => animIndex >= animLines.Length;
	

	/// <summary>
	/// Play the animation for both characters at specified position in sequence of files.
	/// </summary>
	/// <param name="animationIndex"></param>
	void StartAnimation(int animationIndex) 
    {
		MoshAnimation[] animations = GetAnimationsFromFile(animationIndex);
		Character1.StartAnimation(animations[0]);
		Character2.StartAnimation(animations[1]);
	}

	MoshAnimation[] GetAnimationsFromFile(int animationIndex) {
		//TODO maybe better way to store list of animations? Needs to be MatLab-friendly for Niko.
		string[] fileNames = animLines[animationIndex].Split (' '); //Space delimited
		MoshAnimation[] animations = new MoshAnimation[fileNames.Length];
		for (int index = 0; index < fileNames.Length; index++) {
			string filename = fileNames[index];
			string animationFileString = LoadAnimFileAsString(filename);
			animations[index] = new MoShAnimationFromJSON(animationFileString).Build();
		}
		return animations;
	}

	string LoadAnimFileAsString(string filename) {
		string animFilePath1 = Path.Combine(AnimFolder, filename);
        string animText1 = File.ReadAllText(animFilePath1);
        return animText1;
    }



 
    
}