using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoshPlayer.Scripts.BML.Display;
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
		bool doneLoading = false;
		bool started = false;
		bool notYetNotified = true;
		void Awake() {
			if (!File.Exists(AnimationsToPlayFile)) throw new IOException($"Can't find List of Animations file {AnimationsToPlayFile}");
			
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(AnimationsToPlayFile, Settings, AnimFolder, DoneLoading);
			
		}

		void DoneLoading(List<MoshAnimation[]> animationSequence) {
			doneLoading = true;
			moshAnimationPlayer = new MoshAnimationPlayer(animationSequence, Settings, DisplayPointLights, DisplayBones, DisplayMesh);
			Destroy(loader);
		}
		
		void Update() {
			if (!doneLoading) return;
			if (moshAnimationPlayer.AllAnimsComplete) return;

			
			if (!started && notYetNotified) {
				Debug.Log("Waiting to start playing...");
				notYetNotified = false;
			}
			
			if (NextTrialKeys.Any(Input.GetKeyDown)) {
				if (!started) {
					moshAnimationPlayer.StartPlaying();
					started = true;
				}
				moshAnimationPlayer.GoToNextAnimation();
			}
		}

	}
}