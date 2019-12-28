# Blender Importer

This simple tool is an Unity Asset Post Processor for .blend files.    
Unity natively handles the import of .blend files by converting .blend files in .fbx files by using Blender in command line and importing the .fbx.    
This process does no takes into account the differences between Blender and Unity : Blender works with Z up, while Unity uses Y as the up axis. This tool will automatically modify mesh coordinates and transforms to have your Blender model properly oriented in Unity, with the Z up.   

**It doesn't work yet for Skinned Meshes !**

### In Blender, the Z is up :
![Blender](https://raw.githubusercontent.com/ogxd/blender-importer-unity/master/Demo/blender.png)

### In Unity, by default, the Y is up : the model is misoriented
![Before](https://raw.githubusercontent.com/ogxd/blender-importer-unity/master/Demo/before.png)

### In Unity, with the tool, the Z is up : the model is properly oriented
![After](https://raw.githubusercontent.com/ogxd/blender-importer-unity/master/Demo/after.png)

## How To
Download the .unitypackage in the Release tab (or checkout and copy the Assets/Plugins/BlenderImporter folder to your Assets folder)