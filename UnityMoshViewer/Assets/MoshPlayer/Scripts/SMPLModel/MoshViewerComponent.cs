using System.Collections.Generic;
using System.IO;
using MoshPlayer.Scripts.Display;
using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.SMPLModel {
	public class MoshViewerComponent : MonoBehaviour {
		[FormerlySerializedAs("Models")]
		[FormerlySerializedAs("SettingsMain")]
		[SerializeField]
		Models models = default;

		MoshAnimationPlayer moshAnimationPlayer;

		[FormerlySerializedAs("PlaybackOptions")] [SerializeField]
		PlaybackOptions playbackOptions = default;

		[FormerlySerializedAs("characterRenderRenderOptions")]
		[FormerlySerializedAs("CharacterRenderRenderOptions")]
		[FormerlySerializedAs("CharacterRenderOptions")]
		[SerializeField]
		CharacterRenderOptions characterRenderOptions = default;

		[SerializeField]
		CharacterDisplayOptions characterDisplayOptions = default;
		
		AnimationLoader loader;
		bool doneLoading = false;
		bool started = false;
		bool notYetNotified = true;

		void OnEnable() {
			PlaybackEventSystem.OnNextAnimation += GoToNextAnimation;
			PlaybackEventSystem.OnPreviousAnimation += GoToPrevAnimation;
			PlaybackEventSystem.OnRestartAnimations += RestartAnimations;
			PlaybackEventSystem.OnLoadAnimations += LoadAnimations;
			PlaybackEventSystem.OnLoadSingleAnimation += LoadSingleAnimation;
			PlaybackEventSystem.OnLoadNewAnimations += LoadNewAnimations;
			
			PlaybackEventSystem.OnMeshDisplayStateChanged += MeshDisplayStateChanged;
			PlaybackEventSystem.OnBoneDisplayStateChanged += BoneDisplayStateChanged;
			PlaybackEventSystem.OnPointLightDisplayStateChanged += PointLightDisplayStateChanged;
			PlaybackEventSystem.OnChangeLivePoses += SetLivePoses;
			PlaybackEventSystem.OnChangeLivePoseBlendshapes += SetLivePoseBlendshapes;
			PlaybackEventSystem.OnChangeLiveBodyShape += SetLiveBodyShape;
			PlaybackEventSystem.OnChangeManualPosing += SetManualPosing;
			PlaybackEventSystem.OnChangeSnapToGround += SetSnapToGround;
			PlaybackEventSystem.OnChangeUpdateYTranslation += SetUpdateYTranslation;
			PlaybackEventSystem.OnChangeUpdateXzTranslation += SetUpdateXzTranslation;
			PlaybackEventSystem.OnChangeIndividualizedBody += SetIndividualizedBodyState;
			PlaybackEventSystem.OnChangeLoopState += SetLoopState;

			
		}

	


		void OnDisable() {
			PlaybackEventSystem.OnNextAnimation -= GoToNextAnimation;
			PlaybackEventSystem.OnPreviousAnimation -= GoToPrevAnimation;
			PlaybackEventSystem.OnRestartAnimations -= RestartAnimations;
			PlaybackEventSystem.OnLoadAnimations -= LoadAnimations;
			PlaybackEventSystem.OnLoadSingleAnimation -= LoadSingleAnimation;
			PlaybackEventSystem.OnLoadNewAnimations -= LoadNewAnimations;
			
			PlaybackEventSystem.OnMeshDisplayStateChanged -= MeshDisplayStateChanged;
			PlaybackEventSystem.OnBoneDisplayStateChanged -= BoneDisplayStateChanged;
			PlaybackEventSystem.OnPointLightDisplayStateChanged += PointLightDisplayStateChanged;
			PlaybackEventSystem.OnChangeLivePoses -= SetLivePoses;
			PlaybackEventSystem.OnChangeLivePoseBlendshapes -= SetLivePoseBlendshapes;
			PlaybackEventSystem.OnChangeLiveBodyShape -= SetLiveBodyShape;
			PlaybackEventSystem.OnChangeManualPosing -= SetManualPosing;
			PlaybackEventSystem.OnChangeSnapToGround -= SetSnapToGround;
			PlaybackEventSystem.OnChangeUpdateYTranslation -= SetUpdateYTranslation;
			PlaybackEventSystem.OnChangeUpdateXzTranslation -= SetUpdateXzTranslation;
			PlaybackEventSystem.OnChangeIndividualizedBody -= SetIndividualizedBodyState;
			PlaybackEventSystem.OnChangeLoopState -= SetLoopState;
		}

		

		void LoadNewAnimations() {
			moshAnimationPlayer = null;
			loader = null;
		}

		void LoadAnimations(string listFile, string animationsFolder) {
			
			if (!File.Exists(listFile)) throw new IOException($"Can't find List of Animations file {listFile}");
			
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(listFile, models, playbackOptions, animationsFolder, DoneLoading);
		}

		void LoadSingleAnimation(string singlefile) {
			if (!File.Exists(singlefile)) throw new IOException($"Can't find Animation file {singlefile}");
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(singlefile, models, playbackOptions, DoneLoading);
		}
		
		
		void DoneLoading(List<List<MoshAnimation>> animationSequence) {
			doneLoading = true;
			moshAnimationPlayer = new MoshAnimationPlayer(animationSequence, playbackOptions, characterDisplayOptions, characterRenderOptions);
			Destroy(loader);
			if (playbackOptions.OffsetMultipleAnimations) {
				Debug.LogWarning("Warning, you have selected to offset multiple animations from each other! This could cause unwanted results.", this);;
			}
		}


		void Update() {
			if (!doneLoading) return;
			if (moshAnimationPlayer == null) return;
			if (moshAnimationPlayer.AllAnimsComplete) return;

			if (!started && notYetNotified) {
				string updateMessage = $"Waiting to start playing... press \"Next\" button to continue";
				Debug.Log(updateMessage);
				PlaybackEventSystem.UpdatePlayerProgress(updateMessage);
				notYetNotified = false;
			}
		}

		void GoToNextAnimation() {
			if (moshAnimationPlayer == null) return;
			
			if (!started) {
				moshAnimationPlayer.StartPlayingAnimations();
				started = true;
			}
			else {
				moshAnimationPlayer.GoToNextAnimation();
			}
		}

		void GoToPrevAnimation() {
			if (moshAnimationPlayer == null) return;

			moshAnimationPlayer.GoToPrevAnimation();
			started = true;

		}
		
		void RestartAnimations() {
			if (moshAnimationPlayer == null) return;
			
			if (!started) {
				moshAnimationPlayer.StartPlayingAnimations();
				started = true;
			}
			else {
				moshAnimationPlayer.RestartAnimations();
			}
		}

		void SetUpdateYTranslation(bool changeUpdateYTranslation) {
			characterRenderOptions.UpdateTranslationLiveY = changeUpdateYTranslation;
		}

		void SetUpdateXzTranslation(bool changeUpdateXzTranslation) {
			characterRenderOptions.UpdateTranslationLiveXZ = changeUpdateXzTranslation;
		}


		void SetManualPosing(bool manualPosing) {
			characterRenderOptions.AllowPoseManipulation = manualPosing;
		}

		void SetLiveBodyShape(bool liveBodyShape) {
			characterRenderOptions.UpdateBodyShapeLive = liveBodyShape;
		}

		void SetLivePoseBlendshapes(bool livePoseBlendshapes) {
			characterRenderOptions.UpdatePoseBlendshapesLive = livePoseBlendshapes;
		}

		void SetLivePoses(bool livePoses) {
			characterRenderOptions.UpdatePosesLive = livePoses;
		}

		void PointLightDisplayStateChanged(PointLightDisplayState pointLightDisplayState) {
			characterDisplayOptions.DisplayPointLights = pointLightDisplayState;
		}

		void BoneDisplayStateChanged(BoneDisplayState boneDisplayState) {
			characterDisplayOptions.DisplayBones = boneDisplayState;
		}
		
		void MeshDisplayStateChanged(MeshDisplayState newState) {
			characterDisplayOptions.MeshDisplayState = newState;
		}
		
		void SetIndividualizedBodyState(bool newState) {
			characterRenderOptions.ShowIndividualizedBody = newState;
		}
		
		void SetLoopState(bool state) {
			playbackOptions.Loop = state;
		}

		void SetSnapToGround(GroundSnapType snaptype) {
			characterRenderOptions.GroundSnap = snaptype;
		}
		
	}
	
}