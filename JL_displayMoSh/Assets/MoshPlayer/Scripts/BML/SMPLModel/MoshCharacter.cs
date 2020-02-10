using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public interface MoshCharacter {
        SkinnedMeshRenderer SkinnedMeshRender { get; }
        
        SettingsMain SettingsMain { get; }
        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get;  }
        ModelDefinition Model { get; }
        CharacterEvents Events { get; }
        bool SetFeetOnGround { get; }
        Vector3 CombinedOffset { get; }
        Gender Gender { get; }
        void StartAnimation(MoshAnimation moshAnimation, SettingsMain settingsMain);
        void InterruptAnimation();
    }
}