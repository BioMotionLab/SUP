using System.Collections.Generic;
using System.IO;
using MoshPlayer.Scripts.Display;
using MoshPlayer.Scripts.Playback;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoshPlayer.Scripts.SMPLModel {
	public class MoshViewerComponent : MonoBehaviour {

		[SerializeField]
		Models models = default;

		
		[SerializeField]
		BodyOptions bodyOptions = default;

		public BodyOptions BodyOptions { get; private set; }

		[FormerlySerializedAs("characterse")] [FormerlySerializedAs("characterDisplayOptions")] [SerializeField]
		DisplaySettings characterSettings = default;

		public DisplaySettings CharacterSettings { get; private set; }

		[SerializeField]
		PlaybackSettings playbackSettings = default;
		
		public PlaybackSettings PlaybackSettings { get; private set; }
		
		MoshAnimationPlayer moshAnimationPlayer;
		
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

			BodyOptions = Instantiate(bodyOptions);
			CharacterSettings = Instantiate(characterSettings);
			PlaybackSettings = Instantiate(playbackSettings);
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
			loader.Init(listFile, models, PlaybackSettings, animationsFolder, DoneLoading);
		}

		void LoadSingleAnimation(string singlefile) {
			if (!File.Exists(singlefile)) throw new IOException($"Can't find Animation file {singlefile}");
			loader = gameObject.AddComponent<AnimationLoader>();
			loader.Init(singlefile, models, PlaybackSettings, DoneLoading);
		}
		
		
		void DoneLoading(List<List<MoshAnimation>> animationSequence) {
			doneLoading = true;
			moshAnimationPlayer = new MoshAnimationPlayer(animationSequence, PlaybackSettings, CharacterSettings, BodyOptions);
			Destroy(loader);
			if (PlaybackSettings.OffsetMultipleAnimations) {
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
			BodyOptions.UpdateTranslationLiveY = changeUpdateYTranslation;
		}

		void SetUpdateXzTranslation(bool changeUpdateXzTranslation) {
			BodyOptions.UpdateTranslationLiveXZ = changeUpdateXzTranslation;
		}


		void SetManualPosing(bool manualPosing) {
			BodyOptions.AllowPoseManipulation = manualPosing;
		}

		void SetLiveBodyShape(bool liveBodyShape) {
			BodyOptions.UpdateBodyShapeLive = liveBodyShape;
		}

		void SetLivePoseBlendshapes(bool livePoseBlendshapes) {
			BodyOptions.UpdatePoseBlendshapesLive = livePoseBlendshapes;
		}

		void SetLivePoses(bool livePoses) {
			BodyOptions.UpdatePosesLive = livePoses;
		}

		void PointLightDisplayStateChanged(PointLightDisplayState pointLightDisplayState) {
			CharacterSettings.DisplayPointLights = pointLightDisplayState;
		}

		void BoneDisplayStateChanged(BoneDisplayState boneDisplayState) {
			CharacterSettings.DisplayBones = boneDisplayState;
		}
		
		void MeshDisplayStateChanged(MeshDisplayState newState) {
			CharacterSettings.DisplayMeshAs = newState;
		}
		
		void SetIndividualizedBodyState(bool newState) {
			BodyOptions.ShowIndividualizedBody = newState;
		}
		
		void SetLoopState(bool state) {
			PlaybackSettings.Loop = state;
		}

		void SetSnapToGround(GroundSnapType snaptype) {
			BodyOptions.GroundSnap = snaptype;
		}
		
	}
	
}