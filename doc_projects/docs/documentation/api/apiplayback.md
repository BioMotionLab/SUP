---
id: apiplayback
title: Playback Using the API
---

Using the c# scripting api, bmlSUP can easily playback AMASS animations inside your own scripts and projects.

Make sure you have the animations loaded into memory using the [Loading API](apiloading.md). You should have a ```List<List<AMASSAnimation>>``` stored in memory

## Creating a Player object

first, when your player script first initializes, we need to create a new ```SUPPlayer``` object. We need to pass it default settings. These settings can be overridden later. 

```c#
SUPPlayer player = new SUPPlayer(playbackSettings, displaySettings, bodySettings);
```

If we would like to position our animations in the scene, we can supply an optional ```Transform``` origin object. The animations be created as children of that transform such that they will be played in local space of that transform. This also works for rotated origins. 

```c#
SUPPlayer player = new SUPPlayer(playbackSettings, displaySettings, bodySettings, origin);
```

## Playing animations

Once we have a player object, we can use it to play individual animation sets from our loaded list.

```c#
// plays the first animation (index 0)
player.Play(animations[0]);
```

When playing a set, we can override the default settings for that set only by supplying new settings and origin objects.
```c#
player.Play(animations[0], playbackSettings, displaySettings, bodySettings, origin);
```

## Working complete example:

This example creates a component that you can attach to any GameObject in your scene. Once you configure its inspector with the correct settings files, and Animation List, it is ready to go.

The component will initialize itself by creating a player, then loading all the animations into memory. As soon as loading is complete it will start playing the first animation set. Then whenever the user presses ```space```, it will play the next animation set in the list. It will restart when it reaches the end of the list.

```c#
using System.Collections.Generic;
using FileLoaders;
using Playback;
using Settings;
using UnityEngine;

public class AnimationManager : MonoBehaviour {
    
    [SerializeField] AnimationListAsset animationListAsset = default;
    [SerializeField] PlaybackSettings playbackSettings = default;
    [SerializeField] BodySettings bodySettings;
    [SerializeField] DisplaySettings displaySettings;
    
    List<List<AMASSAnimation>> animations;
    SUPPlayer player;

    int currentlyPlayingIndex = 0;
    
    void OnEnable() {
        
        //initialize player
        player = new SUPPlayer(playbackSettings, displaySettings, bodySettings);
        
        // load animations from list asset
        SUPLoader.LoadFromListAssetAsync(animationListAsset, DoneLoading);
        
    }

    void DoneLoading(List<List<AMASSAnimation>> loadedAnimations) {
        
        // save loaded animations to memory
        this.animations = loadedAnimations;
        
        // Start playing first animation when loading complete
        player.Play(animations[0]);
    }

    void Update() {
        
        // Check if space bar pressed.
        if (Input.GetKeyDown(KeyCode.Space)) {
            
            // Stop currently playing animation
            player.StopCurrentAnimations();
            
            // Increment our animation index
            currentlyPlayingIndex++;
            
            // If reached end, restart all.
            if (currentlyPlayingIndex >= animations.Count) currentlyPlayingIndex = 0;
            
            // Play the next animation
            player.Play(animations[currentlyPlayingIndex]);
        }
    }
}
```

