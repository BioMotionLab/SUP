---
id: Troubleshooting 
title: Troubleshooting
---



If the below solutions don't fix your issues, if you think you encounter a bug, or have a feature request, visit our [Github Issues](https://github.com/BioMotionLab/SUP/issues) page. Remember that we're a small team with limited time, so please be patient.


## I'm getting compiler errors

* Make sure your project settings are set up for API Compatibility .Net 4.x. (Edit > Project Settings > Player > Other Settings)
* Make sure TextMeshPro and bmlSUP are installed in the Package Manager.


## The UI text is too big or misaligned

* This is caused by how unity imports packages. I've filed a couple bug reports, but there's no fix in sight.
* It happens if TextMeshPro is imported after bmlSUP, causing all the fonts to be messed up.
* To Fix, in the unity editor Project view, navigate to Packages/bmlSUP Right click on the folder and select "Reimport". 
* This can be avoided in the future by importing TextMeshPro essentials BEFORE importing bmlTUX package.
* If this does not fix the problem please report the issue on our [issues](https://github.com/BioMotionLab/TUX/issues) page stating your current versions of Unity, TextmeshPro, and bmlTUX.


