using Display;
using Playback;
using Settings;

namespace SMPLModel {

    public interface Viewer {
        BodySettings RuntimeBodySettings { get; }
        DisplaySettings RuntimeDisplaySettings { get; }
        PlaybackSettings RuntimePlaybackSettings { get; }
    }
    
    public class UserModifiedSettingsHandler {
        Viewer viewer;

        public UserModifiedSettingsHandler(Viewer viewer) {
            this.viewer = viewer;
            
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

        public void Destroy() {
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

        void SetUpdateYTranslation(bool changeUpdateYTranslation) {
            viewer.RuntimeBodySettings.UpdateTranslationLiveY = changeUpdateYTranslation;
        }

        void SetUpdateXzTranslation(bool changeUpdateXzTranslation) {
            viewer.RuntimeBodySettings.UpdateTranslationLiveXZ = changeUpdateXzTranslation;
        }

        void SetManualPosing(bool manualPosing) {
            viewer.RuntimeBodySettings.AllowPoseManipulation = manualPosing;
        }

        void SetLiveBodyShape(bool liveBodyShape) {
            viewer.RuntimeBodySettings.UpdateBodyShapeLive = liveBodyShape;
        }

        void SetLivePoseBlendshapes(bool livePoseBlendshapes) {
            viewer.RuntimeBodySettings.UpdatePoseBlendshapesLive = livePoseBlendshapes;
        }

        void SetLivePoses(bool livePoses) {
            viewer.RuntimeBodySettings.UpdatePosesLive = livePoses;
        }

        void PointLightDisplayStateChanged(PointLightDisplayState pointLightDisplayState) {
            viewer.RuntimeDisplaySettings.DisplayPointLights = pointLightDisplayState;
        }

        void BoneDisplayStateChanged(BoneDisplayState boneDisplayState) {
            viewer.RuntimeDisplaySettings.DisplayBones = boneDisplayState;
        }

        void MeshDisplayStateChanged(MeshDisplayState newState) {
            viewer.RuntimeDisplaySettings.DisplayMeshAs = newState;
        }

        void SetIndividualizedBodyState(bool newState) {
            viewer.RuntimeBodySettings.ShowIndividualizedBody = newState;
        }

        void SetLoopState(bool state) {
            viewer.RuntimePlaybackSettings.Loop = state;
        }

        void SetSnapToGround(GroundSnapType snaptype) {
            viewer.RuntimeBodySettings.GroundSnap = snaptype;
        }
    }
}