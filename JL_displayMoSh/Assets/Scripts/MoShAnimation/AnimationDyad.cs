using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class AnimationDyad {
    [FormerlySerializedAs("Animation1")]
    [SerializeField]
    TextAsset First;
    
    [FormerlySerializedAs("Animation2")]
    [SerializeField]
    TextAsset Second;


    public AnimationDyad(TextAsset first, TextAsset second) {
        this.First = first;
        this.Second = second;
    }
    
    
}