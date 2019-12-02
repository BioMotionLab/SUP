using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML {
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


		[FormerlySerializedAs("NextTrialKey")]
		[SerializeField]
		KeyCode[] NextTrialKeys = default;

		void Awake() {
			if (!File.Exists(AnimationsToPlayFile)) throw new IOException($"Can't find List of Animations file {AnimationsToPlayFile}");
			
			string[] animLines = File.ReadAllLines(AnimationsToPlayFile);

			List<MoshAnimation[]> animationSequence = new List<MoshAnimation[]>();
			
			foreach (string line in animLines) {
				MoshAnimation[] allAnimationsInThisLine = GetAnimationsFromLine(line);
				animationSequence.Add(allAnimationsInThisLine);
			}

			moshAnimationPlayer = new MoshAnimationPlayer(animationSequence, Settings);
		}
		
		
		MoshAnimation[] GetAnimationsFromLine(string line) {
			//TODO maybe better way to store list of animationSequence? Needs to be MatLab-friendly for Niko.
			string[] fileNames = line.Split (' '); //Space delimited
			MoshAnimation[] animations = new MoshAnimation[fileNames.Length];
			for (int index = 0; index < fileNames.Length; index++) {
				string filename = fileNames[index];
				string animationFileString = LoadAnimFileAsString(filename);
				animations[index] = new MoshAnimationFromJSON(animationFileString).BuildWithSettings(Settings);
			}
			return animations;
		}
		
		string LoadAnimFileAsString(string filename) {
			string animFilePath = Path.Combine(AnimFolder, filename);
			string animText1 = File.ReadAllText(animFilePath);
			return animText1;
		}
		
		void Update() {
			if (moshAnimationPlayer.AllAnimsComplete) return;

			if (NextTrialKeys.Any(Input.GetKeyDown)) {
				moshAnimationPlayer.GoToNextAnimation();
			}
		}

	}
}