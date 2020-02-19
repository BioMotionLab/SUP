using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.SMPLModel {
	public class MoshViewerComponent : MonoBehaviour {
		[SerializeField]
		SettingsMain SettingsMain = default;

		MoshAnimationPlayer moshAnimationPlayer;

		[FormerlySerializedAs("playbackOptions")]
		[SerializeField]
		PlaybackOptions PlaybackOptions = default;
		
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
			loader.Init(listFile, SettingsMain, PlaybackOptions, animationsFolder, DoneLoading);
		}


		void DoneLoading(List<List<MoshAnimation>> animationSequence) {
			doneLoading = true;
			moshAnimationPlayer = new MoshAnimationPlayer(animationSequence, SettingsMain, PlaybackOptions);
			Destroy(loader);
		}
		
		
		void Update() {
			if (!doneLoading) return;
			if (moshAnimationPlayer == null) return;
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
			if (moshAnimationPlayer == null) return;
			
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