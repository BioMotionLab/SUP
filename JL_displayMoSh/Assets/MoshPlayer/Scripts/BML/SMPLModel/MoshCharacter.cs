using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public interface MoshCharacter {
        SkinnedMeshRenderer SkinnedMeshRender { get; }
        
        SMPLSettings Settings { get; }
        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get;  }
        void StartAnimation(MoshAnimation moshAnimation, SMPLSettings settings);
        void InterruptAnimation();
    }
}