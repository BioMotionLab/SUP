using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoshPlayer.Scripts.BML.Display;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.BML.SMPLModel {
	public class MoshViewerComponent : MonoBehaviour {
	
		const string DefaultSelectPathText = "Select...";
	
		[Tooltip("Path to file with list of animation file names")]
		[SerializeField] 
		public string AnimationsToPlayFile = DefaultSelectPathText;
	
		[SerializeField] 
		public string AnimFolder = DefaultSelectPathText;
	
		[SerializeField]
		SMPLSettings Settings = default;

		MoshAnimationPlayer moshAnimationPlayer;

		[SerializeField]
		DisplayBones DisplayBones = default;

		[SerializeField]
		DisplayMesh DisplayMesh = default;
		
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

		void DoneLoading(List<List<MoshAnimation>> animationSequence) {
			doneLoading = true;
			moshAnimationPlayer = new MoshAnimationPlayer(animationSequence, Settings, DisplayBones, DisplayMesh);
			Destroy(loader);
		}
		
		void Update() {
			if (!doneLoading) return;
			if (moshAnimationPlayer.AllAnimsComplete) return;

			
			if (!started && notYetNotified) {
				Debug.Log($"Waiting to start playing... press {NextTrialKeys[0]} to continue");
				notYetNotified = false;
			}
			
			if (NextTrialKeys.Any(Input.GetKeyDown)) {
				if (!started) {
					moshAnimationPlayer.StartPlayingAnimations();
					started = true;
				}
				else {
					moshAnimationPlayer.GoToNextAnimation();
				}
				
			}
		}

	}
}