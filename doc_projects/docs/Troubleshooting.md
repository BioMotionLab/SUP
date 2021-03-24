---
id: Troubleshooting 
title: Troubleshooting
---



If the below solutions don't fix your issues, if you think you encounter a bug, or have a feature request, visit our [Github Issues](https://github.com/BioMotionLab/SUP/issues) page. Remember that we're a small team with limited time, so please be patient.


## I'm getting compiler errors

* Make sure your project settings are set up for API Compatibility .Net 4.x. (Edit > Project Settings > Player > Other Settings)
* Make sure TextMeshPro and bmlSUP are installed in the Package Manager.


## The UI text is too big or misaligned

This is caused by how unity imports packages. I've filed a couple bug reports, but there's no fix in sight. It happens if TextMeshPro is imported after bmlSUP, causing all the fonts to be messed up.

1. In the Unity Editor Project View, navigate to Packages/bmlSUP, right-click on the folder and select "Reimport". 
2. This can be avoided in the future by importing TextMeshPro essentials BEFORE importing bmlTUX package.
3. If this does not fix the problem please report the issue on our [issues](https://github.com/BioMotionLab/TUX/issues) page stating your current versions of Unity, TextMeshPro, and bmlTUX.


## I built/compiled my bmlSUP project as an OSX app, but when I share it no one can run it.

Because of apple's crazy developer policies, you can't open unnotorized apps by default. You will need to use terminal to modify its executable status. This affects MacOS Catalina and later.

1. Move the compiled ```.app``` file into the applications folder.
2. Make sure it's not in a subfolder (for now). Open terminal.
3. Type:s
    ```
    chmod +x /Applications/_YOURAPPNAME_.app/Contents/MacOS/*
    ```
4. Try to open it, you'll probably still get an error. Go to System Preferences > Security and allow the app. Try opening again.

Alternatively, you can set up a free personal mac developer account, configure the Unity Editor to use the profile, and have it sign your app for you.

