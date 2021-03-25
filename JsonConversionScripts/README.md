# bmlSUP conversion scripts

See main project-level readme file for more info: [Main Readme](../README.md)


AMASS is stored as .npz (numpy zip) files. These can only be read in python. Therefore, to use AMASS animations in Unity, they must be converted to a C#-friendly format called JSON. JSON is a human-readable text file that follows certain formatting guidelines to make it easily converted back into computer-friendly data. 

There are two included python notebook example files:

```text
AnimationConverterExamples.ipynb - how to convert single files (read this first)

BatchConvertFolderExamples.ipynb - how to batch-convert an entire folder with subfolders
```

These can be run by any program that can run jupyter notebooks. You can also run them interactively directly on our github page.

If you prefer working in python proper, copy/paste just the example code into a new python file. You likely need at least python 3.6 or higher.