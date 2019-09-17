using UnityEngine;

namespace SMPL.Scripts.mpi {
    public class TransformBackup {
        readonly Transform  parent;
        readonly Vector3    position;
        readonly Quaternion rotation;
        readonly Vector3    localScale;

        public TransformBackup(Transform transform) {
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;
            parent = transform.parent;
        }

        public void RestoreValuesTo(Transform transform) {
            transform.position = position;
            transform.rotation = rotation;
            transform.parent = parent;
            transform.localScale = localScale;
        }

    }
}