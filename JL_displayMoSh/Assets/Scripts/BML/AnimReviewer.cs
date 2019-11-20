using UnityEngine;
using System.IO;
using System;

/// <summary>
/// View a sequence of MoSh animations read from a file, and record right or left.
/// </summary>
public class AnimReviewer : MonoBehaviour {


	string[] animLines;

    public string[][] files;


	/* Since this should be buffered, I think we can write to it whenever we want
     * without performance penalty after opening it once. */
	//Private is apparently the default access modifier in c# and can be omitted
	StreamWriter responseWriter;


    [Tooltip("Path to file with list of animation file names")]
    [HideInInspector]
    public string animListPath = "Assets/Resources/test.txt";
	//public string animListPath;

	[Tooltip("Folder containing MoSh anims")]
    [HideInInspector]
	public string AnimFolder = "Assets/Resources/Animations/";

	[Tooltip("Path to output file for saving responses")]
    [HideInInspector]
	public string responseFilePath = "Assets/responses.csv";

    [Tooltip("Overwrite or append to the response file.")]
    public bool overwrite = true;

    [Tooltip("Only show mesh when anim is playing")]
    public bool hideWhenFinished;

    public bool RecordResponses;

    //public bool RecordResponseTime;

    public bool AllowResponseWhilePlaying = true;

    [Tooltip("Drag objects with MoshCharacter components into these " +
             "fields. Use instances of the MoShViewer prefab.")]
    public MoshCharacter AnimPlayer1, AnimPlayer2;

    //SkinnedMeshRenderer renderer1, renderer2;

    int animIndex = 0;




	void Start () {
		// read the list of filenames.
		animLines = File.ReadAllLines(animListPath);

        Debug.Log(AnimFolder);
        // Resources.Load requires that the pathname be in a folder called Resources. If the path entered is not relative to resources,
        // the whole thing will fail. This is confusing and finicky, so I'm testing for it, and allowing paths to be entered
        // relative to the Resources folder, or relative to the project because I will never remember which one is expected.
        // paths relative to root also seem to work. That's cool. 

        // Ok, so I think the path needs to not contain resources.

        // so first, check if the path isn't empty. If the path is empty, we can just use it as is. 
        if (AnimFolder == "Assets/Resources" || AnimFolder == "Assets/Resources/") {
            AnimFolder = "";
        } else {
            if (!Directory.Exists(AnimFolder)) {
                Debug.LogError("Anim Folder does not exist");
            }

            int substringIndex = AnimFolder.IndexOf("Resources/");

            if (substringIndex >= 0) {
                //resources/ is 10 characters.
                if (substringIndex + 10 < AnimFolder.Length) {
                    AnimFolder = AnimFolder.Substring(substringIndex + 10);
                }
                else {
                    // if there are just a bunch of animation files in the resources 
                    // folder, leave it as the empty string.
                    AnimFolder = "";
                }
            }

            if (AnimFolder[AnimFolder.Length - 1] != '/')
                AnimFolder = AnimFolder + "/";
        }

        if (RecordResponses) {
            
            responseWriter = new StreamWriter(responseFilePath, !overwrite);
            WriteHeader();
        }
		
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

		if (hideWhenFinished) {
			if (AnimPlayer1.AnimDone)
                AnimPlayer1.transform.parent.gameObject.SetActive(false);
			if (AnimPlayer2.AnimDone)
                AnimPlayer2.transform.parent.gameObject.SetActive(false);
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
            
            if (AllowResponseWhilePlaying || (AnimPlayer1.AnimDone && AnimPlayer2.AnimDone)) {
                
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
		string[] filenames = animLines [index].Split (' ');

        // see how many animations are on the specified line and play them. 
        // Supports one or two animations. 
        if (filenames.Length == 2 && filenames[1] != string.Empty) {

            if (hideWhenFinished) {
                AnimPlayer1.transform.parent.gameObject.SetActive(true);
            }
            AnimPlayer2.transform.parent.gameObject.SetActive(true);

            string fpath1 = AnimFolder + filenames[0];
            string fpath2 = AnimFolder + filenames[1];

            //for some reason, extensions must be ommitted.
            TextAsset animText1 = (TextAsset)Resources.Load(fpath1.Substring(0, fpath1.Length - 5));
            TextAsset animText2 = (TextAsset)Resources.Load(fpath2.Substring(0, fpath2.Length - 5));

            AnimPlayer1.PlayAnim(animText1);
            AnimPlayer2.PlayAnim(animText2);
        }
        else {
            
            if (hideWhenFinished) {
                AnimPlayer1.transform.parent.gameObject.SetActive(true);
            }
            AnimPlayer2.transform.parent.gameObject.SetActive(false);

            string fpath1 = AnimFolder + filenames[0];

            TextAsset animText1 = (TextAsset)Resources.Load(fpath1.Substring(0, fpath1.Length - 5)); 
            AnimPlayer1.PlayAnim(animText1);
        }
	}


	void OnApplicationQuit() {
        if (RecordResponses) {
            responseWriter.Dispose();
        }
	}

}