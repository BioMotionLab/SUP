---
id: Converting 
title: Converting to Unity-Readable Format
---


Animations from the AMASS database can be downloaded Here. The animations are stored in ```.npz``` format, which can only be accessed in python. To play the animations in Unity, the files must be converted into a C# readable file format. We provide a python tool to convert into ```.json``` files with some example scripts.

The conversion tools and examples can be accessed From the [Github Page](https://github.com/BioMotionLab/SUP). Download the folder named JsonConversionScripts

Make sure you download the entire folder containing the conversion scripts so that you get all the modules and dependencies. You'll need to run it in an environment that is at least python 3.6, with all the required python libraries installed. 

In the examples, the first few lines run tests to check these dependencies are installed.

A typical workflow:

1. Access and download desired animations from [AMASS](https://amass.is.tue.mpg.de). These are in ```.npz``` format.
2. Run the batch conversion python script on the entire folder of animations to convert them all to ```.json``` format.
3. Load them into bmlSUP.


