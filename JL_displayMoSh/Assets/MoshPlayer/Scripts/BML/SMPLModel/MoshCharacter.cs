using UnityEngine;

namespace MoshPlayer.Scripts.BML {
    public interface MoshCharacter {
        SkinnedMeshRenderer SkinnedMeshRender { get; }
        
        SMPLSettings Settings { get; }
        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get;  }
    }
}