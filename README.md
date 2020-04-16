# MoshViewer

![screenshot](Images/MoshViewer%20Capture.PNG)

Moshviewer is a tool to display animations from the [AMASS database](https://amass.is.tue.mpg.de/) in Unity. 



It reads and plays animation files with several render settings and allows using SMPL-H models in Unity for any purpose. 

Features:
* Simple controls to move the camera that match the controls in the Unity Editor Scene View. 
* Simple controls for playback of the animations including: 
    * Play/Pause
    * Playback speed
    * A "scrubber" to jump to a particular part of an animation.
* Can toggle between rendering modes:
    * Opaque mesh, semi-transparent mesh
    * Bones
    * Point-lights for joints.
* Play multiple animations at once
* Save notes about animations while viewing them using the Review Panel.

To show/hide the entire user-interface (UI) press the "U" key. This is useful for screen capturing and recording.

## Using the player

The AMASS data comes in .npz format, which is not readable by Unity. This repository contains a python tool to convert .npz files into a Unity-friendly .hf (preferred) or .json files. The folder containing the scripts is contained within the root directory of the repository. There is a [Jupyter Notebook](https://github.com/BioMotionLab/UpdatingMoshToSMPLH/blob/master/NumpyToJSONConversionForAMASS/AMASSConverterExamples.ipynb) in this folder with example code demonstrating how to use the conversion scripts.

* **.h5:** Faster, smaller, binary (not human-readable), edit in python
* **.json:** Slower, bigger, human-readable, edit in any text editor. 

## Loading animations

Upon running the program a load screen will appear. Animations can be loaded as single file, a batch of files, or alternatively there are some included samples.


### Batch running:

To run the MoshViewer application on several files in sequence  , the programs needs a path to a folder containing animations in the correct format, and a text file containing a list of which animations to play in that folder and in what order.

The format of the list file should be:

```
animation1.json
animation2.json
```

You can also play multiple animations together by entering them on the same line separated by a space.

```
animation1A.json animation1B.json
animation2A.json animation2B.json
```

You can also specify files inside subfolders:

```
Set1/animationA.json Set1/animationB.json
Set2/animationA.json Set2/animationB.json
```

