using UnityEngine;

namespace MoshPlayer.Scripts.BML.SMPLModel {
    public class CharacterEvents {
        
        public delegate void BodyChangedEvent(IndividualizedBody body);
        public event BodyChangedEvent OnBodyChanged;

        public void BroadcastBodyChange(IndividualizedBody body) {
            OnBodyChanged?.Invoke(body);
        }

        
        public delegate void RegroundEvent(Vector3 groundOffset);
        public event RegroundEvent OnReground;

        public void BroadcastGroundOffset(Vector3 groundOffset) {
            OnReground?.Invoke(groundOffset);
        }
    }
}