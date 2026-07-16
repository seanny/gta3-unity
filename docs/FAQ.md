# FAQ

## What is the goal of GTA3Unity?
GTA3Unity aims to create an open-source, Unity-based reimplementation of Grand Theft Auto III that loads assets from a legally obtained PC installation. Its primary goal is to preserve the game’s playability on modern desktop platforms while providing a maintainable foundation for compatibility fixes and selected visual improvements.

## What platforms does GTA3Unity run on?
Windows, macOS and Linux are primary targets and this is where the majority of development will be focused on.

Mobile platforms are not currently within the project’s release scope and console releases are not planned.

## What are the system requirements for GTA3Unity

Exact minimum requirements are unknown but we know the minimum requirements for the Unity player based on Unity documentation:
Windows:
- Windows 10 version 21H1 (build 19043) or newer
- x86, x64 architecture with SSE2 instruction set support, Arm64
- DX10, DX11, DX12 or Vulkan capable GPUs

macOS:
- Monterey 12 or newer
- Apple Silicon, x64 architecture with SSE2
- Metal capable Intel and AMD GPUs

Linux:
- Ubuntu 22.04, Ubuntu 24.04
- x64 architecture with SSE2 instruction set support
- OpenGL 3.2+, Vulkan capable GPUs
- Gnome desktop environment running on top of X11 or Wayland windowing system.

## Will this work with Vice City and/or San Andreas?
No. GTA3Unity is currently limited to Grand Theft Auto III. Vice City and San Andreas support may be reconsidered only after GTA III is fully implemented. Contributions adding support for those games are not currently accepted.

## Do I need the original Grand Theft Auto III game to use GTA3Unity?
Yes, you will need a legal copy of Grand Theft Auto III which you can purchase from [Steam](https://store.steampowered.com/app/12100/Grand_Theft_Auto_III/)

## Will you add Multi-player?
Multiplayer is not currently planned. The priority is implementing and validating the complete GTA III single-player experience. Multiplayer will not be considered until that work is complete.

## Will mods be supported?
Mods designed to inject DLL or ASI code into the original GTA III executable will not be compatible with GTA3Unity. The project intends to support replacement models, textures, archives, and other asset modifications where reasonably practical, but exact compatibility has not yet been finalised.

## Is GTA3Unity affiliated with Rockstar Games or Take-Two Interactive?
No. GTA3Unity is an independent, unofficial fan project. It is not affiliated with, endorsed by, or supported by Rockstar Games or Take-Two Interactive.

## Is GTA3Unity currently playable
No. GTA3Unity is currently in an early stage of development and does not yet provide a complete playable version of Grand Theft Auto III.

At the time of writing, the project can load most of the GTA III map and spawn an animated pedestrian model controlled using a player controller based on Unity's Starter Assets Third Person Controller.

Some areas of the map may load incorrectly, and collision support is still incomplete. The project currently uses rendered object meshes for some collision detection rather than the original collision meshes.

## Is the Definitive Edition supported
No. Assets from Grand Theft Auto III – The Definitive Edition are not supported. GTA3Unity is designed around the data formats used by the original PC version of Grand Theft Auto III.

## How are the original files loaded
GTA3Unity reads the original Grand Theft Auto III files from a user-provided installation directory at runtime.

Models, textures, map data, animations, and other supported resources are converted into Unity-compatible runtime representations as they are loaded. The original game installation is not modified.

## Will original GTA III save files work
This has not yet been decided. Compatibility with original Grand Theft Auto III save files may be investigated later, but it is not currently implemented or guaranteed.

## Why does the project use Unity
The project lead has experience working with Unity, making it a practical foundation for developing and maintaining the project.

Unity also provides established tools and systems for rendering, animation, physics, input, audio, user interfaces, profiling, and deployment across multiple desktop platforms. These systems allow the project to focus more heavily on reimplementing Grand Theft Auto III's data formats, gameplay systems, and behaviour.

## Do you accept contributions?
Absolutely! Check [contributing](/CONTRIBUTING.md) for more information.