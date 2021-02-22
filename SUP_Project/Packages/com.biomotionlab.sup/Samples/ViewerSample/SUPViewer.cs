using System.Collections.Generic;
using Display;
using Playback;
using Settings;
using SMPLModel;
using UnityEngine;

namespace Samples.ViewerSample {
	public class SUPViewer : MonoBehaviour, Viewer {

		[SerializeField]
		Models models = default;
		
		[SerializeField]
		BodyOptions bodyOptions = default;
		public BodyOptions RuntimeBodyOptions { get; private set; }

		
		[SerializeField]
		DisplaySettings displaySettings = default;
		public DisplaySettings RuntimeDisplaySettings { get; private set; }

		[SerializeField]
		PlaybackSettings playbackSettings = default;
		public PlaybackSettings RuntimePlaybackSettings { get; private set; }

		[SerializeField] GameObject UI = default;
		
		AnimationLoader loader;
		bool doneLoading = false;
		
		List<List<SUPAnimation>> animationSequence;
		bool AllAnimsComplete => currentAnimationIndex >= animationSequence.Count;
		int currentAnimationIndex = 0;
		
		bool started = false;
		bool notYetNotified = true;
		UserModifiedSettingsHandler userModifiedSettingsHandler;
	
		SUPAnimationPlayer animationPlayer;

		void OnEnable() {
			PlaybackEventSystem.OnNextAnimation += GoToNextAnimation;
			PlaybackEventSystem.OnPreviousAnimation += GoToPrevAnimation;
			PlaybackEventSystem.OnRestartAnimations += RestartAnimations;
			PlaybackEventSystem.OnLoadAnimations += LoadAnimations;
			PlaybackEventSystem.OnLoadSingleAnimation += LoadSingleAnimation;
			PlaybackEventSystem.OnLoadNewAnimations += LoadNewAnimations;
			
			userModifiedSettingsHandler = new UserModifiedSettingsHandler(this);
			UI.SetActive(true);
		}

		void Awake() {
			CacheRuntimeSettings();
			animationPlayer = new SUPAnimationPlayer(RuntimePlaybackSettings, RuntimeDisplaySettings, RuntimeBodyOptions);
		}
		
		
		void OnDisable() {
			PlaybackEventSystem.OnNextAnimation -= GoToNextAnimation;
			PlaybackEventSystem.OnPreviousAnimation -= GoToPrevAnimation;
			PlaybackEventSystem.OnRestartAnimations -= RestartAnimations;
			PlaybackEventSystem.OnLoadAnimations -= LoadAnimations;
			PlaybackEventSystem.OnLoadSingleAnimation -= LoadSingleAnimation;
			PlaybackEventSystem.OnLoadNewAnimations -= LoadNewAnimations;
			
			userModifiedSettingsHandler.Destroy();
		}


		void CacheRuntimeSettings() {
			RuntimeBodyOptions = Instantiate(bodyOptions);
			RuntimeDisplaySettings = Instantiate(displaySettings);
			RuntimePlaybackSettings = Instantiate(playbackSettings);
		}

		void LoadNewAnimations() {
			currentAnimationIndex = 0;
			loader = null;
		}

		void LoadAnimations(string listFile, string animationsFolder) {
			loader = gameObject.AddComponent<AnimationLoader>();
			AnimationFileReference fileReference = new AnimationFileReference(listFile, animationsFolder);
			loader.OnDone += DoneLoading;
			loader.Load(fileReference, models, RuntimePlaybackSettings);
		}

		void LoadSingleAnimation(string singleFile) {
			loader = gameObject.AddComponent<AnimationLoader>();
			AnimationFileReference fileReference = new AnimationFileReference(singleFile);
			loader.OnDone += DoneLoading;
			loader.Load(fileReference, models, RuntimePlaybackSettings);
		}


		void DoneLoading(List<List<SUPAnimation>> loadedAnimationSequence) {
			loader.OnDone -= DoneLoading;
			animationSequence = loadedAnimationSequence;
			doneLoading = true;
			Destroy(loader);
			if (RuntimePlaybackSettings.OffsetMultipleAnimations) {
				Debug.LogWarning("Warning, you have selected to offset multiple animations from each other! This could cause unwanted results.", this);
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
		/// Play the animation for characters at specified position in sequence of files.
		/// </summary>
		void StartCurrentAnimationSet() {
			List<SUPAnimation> animationSet = animationSequence[currentAnimationIndex];
			PlaybackEventSystem.PlayingNewAnimationSet(animationSet);

			string updateMessage = $"\tPlaying animation set {currentAnimationIndex+1} of {animationSequence.Count}. " +
			                       $"({animationSet.Count} chars)";
			Debug.Log(updateMessage);
			PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
			animationPlayer.PlaySet(animationSet);
		}


		void StartPlayingAnimations() {
			StartCurrentAnimationSet(); //play the first animation!
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
					Debug.Log(updateMessage);
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
	
}