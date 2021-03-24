---
id: Installation
title: Installation
---

## Requirements
Here is a list of requirements to run and successfully use bmlSUP.

* Basic experience creating and opening Unity projects.
* Unity 2020.1 or later.
   * TextMeshPro 3.0.0 or later.
* **Highly Recommended:** [JetBrains Rider IDE](https://www.jetbrains.com/rider/?fromMenu)
   * Free for academic use.
   * Similar functionality to Visual Studio.
   * Has a beautiful interface.
   * Is highly optimized for Unity development.

## Prepare a project:

1. Create a new Unity Project
2. Make sure you are targeting .NET 4.x in the Project Settings > Player > Other
3. Make sure you have TextMeshPro version 3.0.0 or later installed. 
   1. Go to Window > Package Manager, and look for TextMeshPro in the list, and install it. 
   2.  _Important:_ Make sure TextMeshPro Essentials are imported *BEFORE* you continue.
   3. Go to Window > TextMeshPro > Import Essentials. Doing this after installing bmlTUX can cause display issues.

## Install the package

There are two ways to install packages in unity: Through a package registry or as a folder. Installing through a registry is preferred since it is less error-prone and will allow for easy future updating using the package manager interface.

### Install through a package registry (recommended):

Requires Unity 2020.1 and later.

1. *Important:* Make sure TextMeshPro Essentials are already imported (see above). 
2. Navigate to Edit > Project Settings, and select the Package Manager tab.
3. In the section called Scoped Registries, click the + button to add a new entry. Type in the following information:
    ```text
    Name: BioMotionLab
    URL: https://registry.npmjs.org
    Scope(s): com.biomotionlab.sup
    ```
4. Then open the Package Manager (Window > Package Manager)
5. From the dropdown pick "My Registries". You will see a new entry listed. Install the latest version of bmlSUP. 

Future updates will appear in the package manager as a new version number. Just click the update button.


### Install as a folder (definitely not recommended).

This installation method has several shortcomings. 
* It is more difficult to update to newer versions.
* It pollutes your Assets folder.
* It increases compile time.
* It is much easier to accidentally break things.

This method works with older Unity versions, but bmlSUP is not tested on anything before Unity 2020.1

1. On the releases page https://github.com/BioMotionLab/SUP/releases, download the most recent version.
2. Unzip if necessary, then drag it into your project's Assets folder.
3. After the import, you should be all set.

To update, delete the folder from assets, and follow the above steps again.
