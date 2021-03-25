using System;
using System.Collections.Generic;
using Display;
using FileLoaders;
using Playback;
using Settings;
using SMPLModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace Samples.SUPViewer {
	public class SUPViewer : MonoBehaviour, Viewer {

		[SerializeField]
		Models models = default;
		
		[FormerlySerializedAs("bodyOptions")] [SerializeField]
		BodySettings bodySettings = default;
		public BodySettings RuntimeBodySettings { get; private set; }

		
		[SerializeField]
		DisplaySettings displaySettings = default;
		public DisplaySettings RuntimeDisplaySettings { get; private set; } 

		
		
		[SerializeField]
		PlaybackSettings playbackSettings = default;
		public PlaybackSettings RuntimePlaybackSettings { get; private set; }
		
		
		[SerializeField] AnimationListAsset samplesListAsset = default;

		bool doneLoading = false;
		
		List<List<AMASSAnimation>> animationSequence;

		bool AllAnimsComplete {
			get {
				if (animationSequence == null) return false;
				return currentAnimationIndex >= animationSequence.Count;
			}
		}

		int currentAnimationIndex = 0;
		
		bool started = false;
		bool notYetNotified = true;
		UserModifiedSettingsHandler userModifiedSettingsHandler;
	
		SUPPlayer animationPlayer;

		

		void OnEnable() {
			PlaybackEventSystem.OnNextAnimation += GoToNextAnimation;
			PlaybackEventSystem.OnPreviousAnimation += GoToPrevAnimation;
			PlaybackEventSystem.OnRestartAnimations += RestartAnimations;
			PlaybackEventSystem.OnLoadAnimations += LoadAnimations;
			PlaybackEventSystem.OnLoadSingleAnimation += LoadSingleAnimation;
			PlaybackEventSystem.OnLoadSamples += LoadSamples;
			PlaybackEventSystem.OnLoadNewAnimations += LoadNewAnimations;
			
			userModifiedSettingsHandler = new UserModifiedSettingsHandler(this);
		}

		void Awake() {
			CacheRuntimeSettings();
			animationPlayer = new SUPPlayer(RuntimePlaybackSettings, RuntimeDisplaySettings, RuntimeBodySettings);
		}
		
		
		void OnDisable() {
			PlaybackEventSystem.OnNextAnimation -= GoToNextAnimation;
			PlaybackEventSystem.OnPreviousAnimation -= GoToPrevAnimation;
			PlaybackEventSystem.OnRestartAnimations -= RestartAnimations;
			PlaybackEventSystem.OnLoadAnimations -= LoadAnimations;
			PlaybackEventSystem.OnLoadSingleAnimation -= LoadSingleAnimation;
			PlaybackEventSystem.OnLoadSamples -= LoadSamples;
			PlaybackEventSystem.OnLoadNewAnimations -= LoadNewAnimations;
			
			
			
			userModifiedSettingsHandler.Destroy();
		}

		


		void CacheRuntimeSettings() {
			RuntimeBodySettings = Instantiate(bodySettings);
			RuntimeDisplaySettings = Instantiate(displaySettings);
			RuntimePlaybackSettings = Instantiate(playbackSettings);
		}

		void LoadNewAnimations() {
			currentAnimationIndex = 0;
		}

		void LoadSamples() {
			SUPLoader.LoadFromListAssetAsync(samplesListAsset, models, playbackSettings, DoneLoading);
		}
		
		void LoadAnimations(string listFile, string animationsFolder) {
			AnimationFileReference fileReference = new AnimationFileReference(listFile, animationsFolder);
			SUPLoader.LoadAsync(fileReference, models, RuntimePlaybackSettings, DoneLoading);
		}

		void LoadSingleAnimation(string singlefile) {
			AnimationFileReference fileReference = new AnimationFileReference(singlefile);
			
			SUPLoader.LoadAsync(fileReference, models, RuntimePlaybackSettings, DoneLoading);
		}


		void DoneLoading(List<List<AMASSAnimation>> loadedAnimationSequence) {
			animationSequence = loadedAnimationSequence;
			if (animationSequence == null) return;
			doneLoading = true;
			if (RuntimePlaybackSettings.OffsetMultipleAnimations) {
				Debug.LogWarning("Warning, you have selected to offset multiple animations from each other! This could cause unwanted results.", this);;
			}
			PlaybackEventSystem.BeginPlayBackState();
		}


		void Update() {
			if (!doneLoading) return;
			if (AllAnimsComplete) return;

			if (!started && notYetNotified) {
				notYetNotified = false;
				if (playbackSettings.PlaybackImmediately) {
					PlaybackEventSystem.GoToNextAnimation();
				}
				else {
					string updateMessage = $"Waiting to start playing... press \"Next\" button to continue";
					Debug.Log(updateMessage);
					PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
				}
				
				
			}
		}

		/// <summary>
		/// Play the samplesListAsset for characters at specified position in sequence of files.
		/// </summary>
		void StartCurrentAnimationSet() {
			List<AMASSAnimation> animationSet = animationSequence[currentAnimationIndex];
			PlaybackEventSystem.PlayingNewAnimationSet(animationSet);

			string updateMessage = $"Playing set {currentAnimationIndex+1} of {animationSequence.Count} from {samplesListAsset.name}. " +
			                       $"( {animationSet.Count.Pluralize("animation", "animations")} in set)";
			Debug.Log(Format.Log(updateMessage));
			PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
			animationPlayer.Play(animationSet);
		}


		public void StartPlayingAnimations() {
			StartCurrentAnimationSet(); //play the first samplesListAsset!
		}
		
		void GoToNextAnimation() {
			
			if (!started) {
				StartPlayingAnimations();
				started = true;
			}
			else {
				animationPlayer.StopCurrentAnimations();
				currentAnimationIndex++;
				if (AllAnimsComplete) {
					string updateMessage = "All Animations Complete";
					Debug.Log(Format.Log(updateMessage));
					PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
					return;
				}
				StartCurrentAnimationSet();
			}
		}

		void GoToPrevAnimation() {

			currentAnimationIndex = currentAnimationIndex - 1;
			if (currentAnimationIndex < 0) {
				currentAnimationIndex = 0;
				return;
			}
			animationPlayer.StopCurrentAnimations();
			StartCurrentAnimationSet();
			started = true;

		}
		
		void RestartAnimations() {

			if (!started) {
				StartPlayingAnimations();
				started = true;
			}
			else {
				Debug.Log("Restarting All Animations");
				currentAnimationIndex = 0;
				animationPlayer.StopCurrentAnimations();
				StartCurrentAnimationSet();
			}
		}
	}

	public static class StringExtensions {
		public static string Pluralize(this int number, string singular, string plural) {
			string word = Math.Abs(number) <= 1 ? singular : plural;
			return $"{number} {word}";
		}
	}
}
	
