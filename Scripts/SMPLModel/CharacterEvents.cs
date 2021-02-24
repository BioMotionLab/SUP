using UnityEngine;

namespace SMPLModel {
    public class CharacterEvents {
        
        public delegate void BodyChangedEvent();
        public event BodyChangedEvent OnBodyChanged;

        public void BodyHasChanged() {
            OnBodyChanged?.Invoke();
        }

        
        public delegate void RegroundEvent(Vector3 groundOffset);
        public event RegroundEvent OnReground;

        public void BroadcastGroundOffset(Vector3 groundOffset) {
            OnReground?.Invoke(groundOffset);
        }
        
    }
}