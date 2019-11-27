using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// View a sequence of MoSh animations read from a file, and record right or left.
/// </summary>
public class AnimReviewer : MonoBehaviour {

    const string DefaultSelectPathText = "Select...";
	
	readonly List<MoshAnimation[]> animations = new List<MoshAnimation[]>();
	
	bool AllAnimsComplete => animIndex >= animations.Count;
	
    [FormerlySerializedAs("animListPath")]
    [Tooltip("Path to file with list of animation file names")]
    [HideInInspector]
    [SerializeField] 
    public string AnimListPath = DefaultSelectPathText;
	
	[Tooltip("Folder containing MoSh anims")]
    [HideInInspector]
    [SerializeField] 
	public string AnimFolder = DefaultSelectPathText;
	
	
	[FormerlySerializedAs("AnimPlayer1")]
    [SerializeField] 
    public MoshCharacter Character1;

    [FormerlySerializedAs("AnimPlayer2")]
    [SerializeField] 
    public MoshCharacter Character2;
	
    int animIndex = 0;
	
	[SerializeField]
	SMPLSettings Settings = default;

	void Start () {
		if (!File.Exists(AnimListPath)) throw new IOException($"Can't find List of Animations file {AnimListPath}");
		string[] animLines = File.ReadAllLines(AnimListPath);
		foreach (string line in animLines) {
			MoshAnimation[] allAnimationsInThisLine = GetAnimationsFromLine(line);
			animations.Add(allAnimationsInThisLine);
		}
		StartAnimation(animIndex); //play the first animation!
	}
	
	MoshAnimation[] GetAnimationsFromLine(string line) {
		//TODO maybe better way to store list of animations? Needs to be MatLab-friendly for Niko.
		string[] fileNames = line.Split (' '); //Space delimited
		MoshAnimation[] animations = new MoshAnimation[fileNames.Length];
		for (int index = 0; index < fileNames.Length; index++) {
			string filename = fileNames[index];
			string animationFileString = LoadAnimFileAsString(filename);
			Debug.Log(filename);
			animations[index] = new MoShAnimationFromJSON(animationFileString).BuildWithSettings(Settings);
		}
		return animations;
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

	/// <summary>
	/// Play the animation for both characters at specified position in sequence of files.
	/// </summary>
	/// <param name="animationIndex"></param>
	void StartAnimation(int animationIndex) {
		MoshAnimation[] animationSet = animations[animationIndex];
		Debug.Log($"Playing animation number {animationIndex}, {animationSet.Length} animations in set");
		Character1.StartAnimation(animationSet[0]);
		Character2.StartAnimation(animationSet[1]);
	}

	string LoadAnimFileAsString(string filename) {
		string animFilePath1 = Path.Combine(AnimFolder, filename);
        string animText1 = File.ReadAllText(animFilePath1);
        return animText1;
    }
	
}