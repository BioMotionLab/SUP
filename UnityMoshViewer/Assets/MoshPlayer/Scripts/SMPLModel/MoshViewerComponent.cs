using System;
using System.Collections.Generic;
using System.IO;
using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.SMPLModel {
	public class MoshViewerComponent : MonoBehaviour {

		[SerializeField]
		Models models = default;
		
		[SerializeField]
		BodyOptions bodyOptions = default;
		public BodyOptions RuntimeBodyOptions { get; private set; }

		
		[SerializeField]
		DisplaySettings characterSettings = default;
		public DisplaySettings RuntimeCharacterSettings { get; private set; }

		
		[SerializeField]
		PlaybackSettings playbackSettings = default;
		public PlaybackSettings RuntimePlaybackSettings { get; private set; }
		
		AnimationLoader loader;
		bool doneLoading = false;
		
		List<List<MoshAnimation>> animationSequence;
		public bool AllAnimsComplete => currentAnimationIndex >= animationSequence.Count;
		int currentAnimationIndex = 0;
		
		bool started = false;
		bool notYetNotified = true;
		UserModifiedSettingsHandler userModifiedSettingsHandler;
		
		List<MoshCharacter> currentCharacters;
		
		void OnEnable() {
			PlaybackEventSystem.OnNextAnimation += GoToNextAnimation;
			PlaybackEventSystem.OnPreviousAnimation += GoToPrevAnimation;
			PlaybackEventSystem.OnRestartAnimations += RestartAnimations;
			PlaybackEventSystem.OnLoadAnimations += LoadAnimations;
			PlaybackEventSystem.OnLoadSingleAnimation += LoadSingleAnimation;
			PlaybackEventSystem.OnLoadNewAnimations += LoadNewAnimations;
			
			userModifiedSettingsHandler = new UserModifiedSettingsHandler(this);
		}

		void Awake() {
			CacheRuntimeSettings();
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
			RuntimeCharacterSettings = Instantiate(characterSettings);
			RuntimePlaybackSettings = Instantiate(playbackSettings);
		}

		void LoadNewAnimations() {
			currentAnimationIndex = 0;
			loader = null;
		}

		void LoadAnimations(string listFile, string animationsFolder) {
			
			if (!File.Exists(listFile)) throw new IOException($"Can't find List of Animations file {listFile}");
			
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(listFile, models, RuntimePlaybackSettings, animationsFolder, DoneLoading);
		}

		void LoadSingleAnimation(string singlefile) {
			if (!File.Exists(singlefile)) throw new IOException($"Can't find Animation file {singlefile}");
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(singlefile, models, RuntimePlaybackSettings, DoneLoading);
		}


		void DoneLoading(List<List<MoshAnimation>> loadedAnimationSequence) {
			animationSequence = loadedAnimationSequence;
			doneLoading = true;
			Destroy(loader);
			if (RuntimePlaybackSettings.OffsetMultipleAnimations) {
				Debug.LogWarning("Warning, you have selected to offset multiple animations from each other! This could cause unwanted results.", this);;
			}
		}


		void Update() {
			if (!doneLoading) return;
			if (AllAnimsComplete) return;

			if (!started && notYetNotified) {
				string updateMessage = $"Waiting to start playing... press \"Next\" button to continue";
				Debug.Log(updateMessage);
				PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
				notYetNotified = false;
			}
		}

		/// <summary>
		/// Play the animation for both characters at specified position in sequence of files.
		/// </summary>
		void StartAnimation() {
			List<MoshAnimation> animationGroup = animationSequence[currentAnimationIndex];

			string updateMessage = $"\tPlaying animation set {currentAnimationIndex+1} of {animationSequence.Count}. " +
			                       $"({animationGroup.Count} chars)";
			Debug.Log(updateMessage);
			PlaybackEventSystem.UpdatePlayerProgress(updateMessage);

			string animationStrings = "";
            
			List<MoshCharacter> newCharacters = new List<MoshCharacter>();
			for (int animationIndex = 0; animationIndex < animationGroup.Count; animationIndex++) {
				MoshAnimation moshAnimation = animationGroup[animationIndex];
				moshAnimation.Reset();
				animationStrings += moshAnimation.AnimationName + " ";
                
				string characterName = $"{moshAnimation.Data.Gender} Character {animationIndex}";
				MoshCharacter moshCharacter = moshAnimation.Data.Model.CreateNewCharacter(characterName, moshAnimation.Data.Gender);
				moshCharacter.SetIndex(animationIndex);
                
                
				newCharacters.Add(moshCharacter);
				moshCharacter.StartAnimation(moshAnimation, RuntimePlaybackSettings, RuntimeCharacterSettings, RuntimeBodyOptions);
			}
			PlaybackEventSystem.PlayingNewAnimationSet(animationStrings.Trim());
            
			currentCharacters = newCharacters;
		}
		
		void StopCurrentAnimations() {
			if (currentCharacters == null) return;
			foreach (MoshCharacter character in currentCharacters) {
				if (character == null) continue;
				character.InterruptAnimation();
			}
		}
		
		
		public void StartPlayingAnimations() {
			StartAnimation(); //play the first animation!
		}
		
		void GoToNextAnimation() {
			
			if (!started) {
				StartPlayingAnimations();
				started = true;
			}
			else {
				StopCurrentAnimations();
				currentAnimationIndex++;
				if (AllAnimsComplete) {
					string updateMessage = "All Animations Complete";
					Debug.Log(updateMessage);
					PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
					return;
				}
				StartAnimation();
			}
		}

		void GoToPrevAnimation() {

			currentAnimationIndex = currentAnimationIndex - 1;
			if (currentAnimationIndex < 0) {
				currentAnimationIndex = 0;
				return;
			}
			StopCurrentAnimations();
			StartAnimation();
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
				StopCurrentAnimations();
				StartAnimation();
			}
		}
	}
	
}