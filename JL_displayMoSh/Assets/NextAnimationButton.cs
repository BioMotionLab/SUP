using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class NextAnimationButton : MonoBehaviour
{


    [PublicAPI]
    public void PlayNextAnimation() {
        PlaybackEventSystem.GoToNextAnimation();
    }
}
