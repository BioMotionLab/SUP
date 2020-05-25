using System.Collections.Generic;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace MoshPlayer {
    [CreateAssetMenu(fileName = "Mesh Display Settings", menuName = "new Mesh Display Settings", order = 0)]
    public class MeshDisplaySettings : ScriptableObject {
        public Material Opaque;
        public Material SemiTransparent;
        public List<Material> OptionalMaterialList = new List<Material>();
    }
}