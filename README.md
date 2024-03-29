<div align="center">
  <h1>
      Unity-Tools
  </h1>
  <p>
     A collection of useful tools for Unity
  </p>
  <p>
     Made by <a href="https://markcreator.net/">Markcreator</a>
  </p>
  
  <br />
</div>

## [SizeProfiler](https://github.com/Markcreator/SizeProfiler/releases)

An interface that lets you inspect any object in Unity and find its dependencies and file size breakdown.

<br />

## [VRCTextureCompressor](https://github.com/Markcreator/VRCTextureCompressor/releases)

A background script that automatically handles texture compression so that VRChat avatars always upload small and optimized.
 
<details>
  <summary>About</summary>
  
> Often VRChat users forget to optimize and compress their avatar textures because it is an easy thing to overlook. It also can be tedious to apply all the correct settings to all textures manually.
>
> This script automatically detects when you are about to upload an avatar and which avatar you are about to upload. It then automatically finds all the textures that avatar uses and compresses them so that your avatar uploads small and optimized.
>
> It also means that avatar creators can include this script in their avatar packages if they want to guarantee that people can never forget to optimize their textures before uploading. (You can also set your textures to not use crunch compression by default, which speeds up your package import time by a lot!)
  
</details>

## [TransformCopy](https://github.com/Markcreator/Unity-Tools/blob/main/Scripts/TransformCopy/)

An interface that lets you copy all the positions, rotations, and scales from one gameobject and its children to another.
Useful for quickly synchronizing an FBX to an existing model in a scene and such.
