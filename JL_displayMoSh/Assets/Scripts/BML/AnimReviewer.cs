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

	/* Since this should be buffered, I think we can write to it whenever we want
     * without performance penalty after opening it once. */
    StreamWriter responseWriter;


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

    [FormerlySerializedAs("hideWhenFinished")]
    [Tooltip("Only show mesh when anim is playing")]
    [SerializeField] 
    public bool HideWhenFinished;

    [SerializeField] 
    public bool RecordResponses;
    
    [SerializeField] 
    public bool AllowResponseWhilePlaying = true;

    
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
		// read the list of filenames.
		animLines = File.ReadAllLines(AnimListPath);
        
        playAnim(animIndex); //play the first animation!
	}


    void WriteHeader() {
        string header = "";
        //if (RecordResponseTime) {
        //    header += "response time,";
        //}
        header += "animation,response";
        responseWriter.WriteLine(header);
    }



	void Update () {

		if (HideWhenFinished) {
			if (Character1.AnimDone)
                Character1.transform.parent.gameObject.SetActive(false);
			if (Character2.AnimDone)
                Character2.transform.parent.gameObject.SetActive(false);
		}

        // Controls to step through the animation. 
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightBracket)) {
            // skip to next animation.
            animIndex++;
            if (animIndex >= animLines.Length) {
                animIndex = animLines.Length - 1;
            }
            else {
                playAnim(animIndex);
            }
        }

        else if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            // skip to previous animation.
            animIndex--;
            playAnim(animIndex);
            if (animIndex < 0) {
                animIndex = 0;
            }
            else {
                playAnim(animIndex);
            }
        }

        else if (Input.GetKeyDown(KeyCode.R)) {
            // replay the animation.
            playAnim(animIndex);
        } 

        else if (RecordResponses) {
            
            if (AllowResponseWhilePlaying || (Character1.AnimDone && Character2.AnimDone)) {
                
                if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                    // record left. Play Next
                    string output = animLines[animIndex] + ", " + "left";
                    responseWriter.WriteLine(output);

                    animIndex++;
                    if (animIndex >= animLines.Length) {
                        animIndex = animLines.Length - 1;
                    }
                    else {
                        playAnim(animIndex);
                    }
                }

                else if (Input.GetKeyDown(KeyCode.RightArrow)) {
                    // record right. Play Next
                    string output = animLines[animIndex] + ", " + "Right";
                    responseWriter.WriteLine(output);

                    animIndex++;
                    if (animIndex >= animLines.Length) {
                        animIndex = animLines.Length - 1;
                    } else {
                        playAnim(animIndex);
                    }


                }
            }

        }
	}


	/// <summary>
	/// Play the animation for both characters at specified position in sequence of files.
	/// </summary>
	/// <param name="index"></param>
	void playAnim(int index) 
    {

        if (index < 0 || index >= animLines.Length) {
            return; // if there are no animations left in the specified direction. 
        }
        
        // space delimited.
		string[] filenames = animLines[index].Split (' ');
        Debug.Log($"filenames = {filenames[0]}, {filenames[1]}");
        // see how many animations are on the specified line and play them. 
        // Supports one or two animations. 
        

        //TODO Assumes dyad at the moment. Make viewer for single files
        Character1.transform.parent.gameObject.SetActive(true);
        Character2.transform.parent.gameObject.SetActive(true);

        string fpath1 = Path.Combine(AnimFolder, filenames[0]);
        string fpath2 = Path.Combine(AnimFolder, filenames[1]);

        //for some reason, extensions must be ommitted.
        string animText1 = File.ReadAllText(fpath1);
        string animText2 = File.ReadAllText(fpath2);

        Character1.PlayAnim(animText1);
        Character2.PlayAnim(animText2);
        
	}

    


	void OnApplicationQuit() {
        if (RecordResponses) {
            responseWriter.Dispose();
        }
	}

 
    
}