using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoshPlayer.Scripts.BML.Display;
using MoshPlayer.Scripts.BML.FileLoaders;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML.SMPLModel {
	public class MoshViewerComponent : MonoBehaviour {
	
		const string DefaultSelectPathText = "Select...";
	
		[FormerlySerializedAs("AnimListPath")]
		[Tooltip("Path to file with list of animation file names")]
		[SerializeField] 
		public string AnimationsToPlayFile = DefaultSelectPathText;
	
		[Tooltip("Folder containing MoSh anims")]
		[SerializeField] 
		public string AnimFolder = DefaultSelectPathText;
	
		[SerializeField]
		SMPLSettings Settings = default;

		MoshAnimationPlayer moshAnimationPlayer;

		[FormerlySerializedAs("displayBones")]
		[SerializeField]
		DisplayBones DisplayBones = default;

		[FormerlySerializedAs("displayMesh")]
		[SerializeField]
		DisplayMesh DisplayMesh = default;

		[FormerlySerializedAs("displayPointLights")]
		[SerializeField]
		DisplayPointLights DisplayPointLights = default;
		
		[FormerlySerializedAs("NextTrialKey")]
		[SerializeField]
		KeyCode[] NextTrialKeys = default;

		AnimationLoader loader;

		void Awake() {
			if (!File.Exists(AnimationsToPlayFile)) throw new IOException($"Can't find List of Animations file {AnimationsToPlayFile}");
			
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(AnimationsToPlayFile, Settings, AnimFolder);
			moshAnimationPlayer = new MoshAnimationPlayer(loader.animationSequence, Settings, DisplayPointLights, DisplayBones, DisplayMesh);
			
		}
		
		void Update() {
			if (!loader.DoneLoading) return;
			if (moshAnimationPlayer.AllAnimsComplete) return;

			if (NextTrialKeys.Any(Input.GetKeyDown)) {
				moshAnimationPlayer.GoToNextAnimation();
			}
		}

	}

	public class AnimationLoader : MonoBehaviour {
		
		string animationsToPlayFile;
		SMPLSettings settings;
		string animFolder;

		public bool           DoneLoading;
		public List<MoshAnimation[]> animationSequence;
		
		
		public void Init(string animationsToPlayFile, SMPLSettings settings, string animFolder) {
			this.animationsToPlayFile = animationsToPlayFile;
			this.settings = settings;
			this.animFolder = animFolder;

			string[] animLines = File.ReadAllLines(animationsToPlayFile);
			
			Debug.Log($"Loading {animLines.Length} animations from files. If there are a lot, this could take a few seconds...");

			animationSequence = new List<MoshAnimation[]>();

			for (int lineIndex = 0; lineIndex < animLines.Length; lineIndex++) {
				string line = animLines[lineIndex];
				MoshAnimation[] allAnimationsInThisLine = GetAnimationsFromLine(line);
				animationSequence.Add(allAnimationsInThisLine);
			}
			
			Debug.Log("Done Loading Animations.");
		}



		MoshAnimation[] GetAnimationsFromLine(string line) {
			//TODO maybe better way to store list of animationSequence? Needs to be MatLab-friendly for Niko.
			string[] fileNames = line.Split (' '); //Space delimited
			MoshAnimation[] animations = new MoshAnimation[fileNames.Length];
			for (int index = 0; index < fileNames.Length; index++) {
				string filename = fileNames[index];
				string animationFileString = LoadAnimFileAsString(filename);
				animations[index] = new MoshAnimationFromJSON(animationFileString).BuildWithSettings(settings);
			}
			return animations;
		}
		
		string LoadAnimFileAsString(string filename) {
			string animFilePath = Path.Combine(animFolder, filename);
			string animText1 = File.ReadAllText(animFilePath);
			return animText1;
		}
	}
}