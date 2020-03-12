using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using MoshPlayer.Scripts.Display;
using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.SMPLModel {
	public class MoshViewerComponent : MonoBehaviour {
		[FormerlySerializedAs("SettingsMain")]
		[SerializeField]
		Models Models = default;

		MoshAnimationPlayer moshAnimationPlayer;

		[FormerlySerializedAs("playbackOptions")]
		[SerializeField]
		PlaybackOptions PlaybackOptions = default;

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
			PlaybackEventSystem.OnLoadAnimations += LoadAnimations;
			
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
		}

		void OnDisable() {
			PlaybackEventSystem.OnNextAnimation -= GoToNextAnimation;
			PlaybackEventSystem.OnPreviousAnimation -= GoToPrevAnimation;
			PlaybackEventSystem.OnLoadAnimations -= LoadAnimations;
			
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
		}

		void LoadAnimations(string listFile, string animationsFolder) {
			
			if (!File.Exists(listFile)) throw new IOException($"Can't find List of Animations file {listFile}");
			
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(listFile, Models, PlaybackOptions, animationsFolder, DoneLoading);
		}


		void DoneLoading(List<List<MoshAnimation>> animationSequence) {
			doneLoading = true;
			moshAnimationPlayer = new MoshAnimationPlayer(animationSequence, PlaybackOptions, characterDisplayOptions, characterRenderOptions);
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

		public void GoToPrevAnimation() {
			if (moshAnimationPlayer == null) return;

			moshAnimationPlayer.GoToPrevAnimation();

		}

		void SetSnapToGround(bool snapToGround) {
			characterRenderOptions.SnapToGroundFirstFrame = snapToGround;
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
	}
	
}