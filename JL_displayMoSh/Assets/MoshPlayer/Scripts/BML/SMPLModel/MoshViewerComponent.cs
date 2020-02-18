using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MoshPlayer.Scripts.BML.Display;
using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
	public class MoshViewerComponent : MonoBehaviour {
	
		const string DefaultSelectPathText = "Select...";
		
		[SerializeField]
		SettingsMain SettingsMain = default;

		MoshAnimationPlayer moshAnimationPlayer;

		[SerializeField]
		PlaybackOptions playbackOptions = default;
		
		AnimationLoader loader;
		bool doneLoading = false;
		bool started = false;
		bool notYetNotified = true;

		void OnEnable() {
			PlaybackEventSystem.OnNextAnimation += GoToNextAnimation;
			PlaybackEventSystem.OnLoadAnimations += LoadAnimations;
		}

		void OnDisable() {
			PlaybackEventSystem.OnNextAnimation -= GoToNextAnimation;
			PlaybackEventSystem.OnLoadAnimations -= LoadAnimations;
		}

		void LoadAnimations(string listFile, string animationsFolder) {
			
			if (!File.Exists(listFile)) throw new IOException($"Can't find List of Animations file {listFile}");
			
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(listFile, SettingsMain, playbackOptions, animationsFolder, DoneLoading);
		}


		void DoneLoading(List<List<MoshAnimation>> animationSequence) {
			doneLoading = true;
			moshAnimationPlayer = new MoshAnimationPlayer(animationSequence, SettingsMain, playbackOptions);
			Destroy(loader);
		}
		
		void Update() {
			if (!doneLoading) return;
			if (moshAnimationPlayer.AllAnimsComplete) return;

			
			if (!started && notYetNotified) {
				string updateMessage = $"Waiting to start playing... press \"Next Animation\" to continue";
				Debug.Log(updateMessage);
				PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
				notYetNotified = false;
			}
		}

		[PublicAPI]
		public void GoToNextAnimation() {
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