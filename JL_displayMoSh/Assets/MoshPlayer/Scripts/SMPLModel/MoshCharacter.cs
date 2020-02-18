using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public interface MoshCharacter {
        SkinnedMeshRenderer SkinnedMeshRender { get; }

        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get;  }
        ModelDefinition Model { get; }
        CharacterEvents Events { get; }
        bool SetFeetOnGround { get; }
        
        CharacterOptions Options { get; }
        CharacterDisplayOptions DisplayOptions { get; }
        Vector3 OffsetErrorBetweenPelvisAndZero { get; }
        Vector3 OffsetErrorInFbxBetweenRigAndMesh { get; }
        Vector3 CombinedOffset { get; }
        Gender Gender { get; }
        void StartAnimation(MoshAnimation moshAnimation, SettingsMain settingsMain, PlaybackOptions playbackOptions);
        void InterruptAnimation();
    }
}