using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Display {
    
    [Serializable]
    public class MeshDisplaySettings {
        public Material Opaque;
        public Material SemiTransparent;
        public List<Material> OptionalMaterialList = new List<Material>();
    }
}