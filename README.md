# GTA3-Unity

GTA3-Unity is a Unity project that aims to create a fully playable version of GTA III inside the Unity game engine, with goals similar to those of re3 and OpenRW.

Both projects made significant contributions:

* re3 successfully achieved its primary goals, but its use of reverse-engineered code resulted in legal action from Take-Two.
* OpenRW appears to have become inactive after development on re3 gained momentum.

This is not intended as criticism of either project. Both accomplished valuable work, and re3 fully achieved its stated goals.

A legally obtained copy of GTA III for PC is required to use this project.

## Installation

GTA3-Unity requires assets from the PC version of GTA III. You must own a legal copy of the game.

[GTA III on Steam](https://store.steampowered.com/app/12100/Grand_Theft_Auto_III/)

## Primary Goals

The project’s primary goals are:

* Make GTA III playable from start to finish, including all side content.
* Support the desktop platforms supported by Unity.

## Secondary Goals

Implement selected improvements previously provided by re3, including:

* A debug menu accessible with `Ctrl+M`.
* Support for loading DFF and TXD files from non-PC versions of the game.

## Non-Goals

To prevent scope creep, the following are not currently within the project’s scope:

* Grand Theft Auto: Vice City and Grand Theft Auto: San Andreas support.
* Grand Theft Auto: Liberty City Stories and Grand Theft Auto: Vice City Stories support.

Support for these games may be considered after GTA III support is complete. Until then, pull requests implementing them will not be accepted.

## Modding

Because the project is still in an early stage of development, the final extent of mod support has not yet been determined.

The current expectations are:

* Asset modifications, such as replacement models and textures, should work as similarly to the original GTA III as reasonably possible.
* Mods that depend on DLL or ASI injection will not work because of the architecture of this project.

## Credits

* [mukaschultze/grand-theft-auto-for-unity](https://github.com/mukaschultze/grand-theft-auto-for-unity) by mukaschultze, from which portions of code were used. The project is identified as MIT-licensed in [this issue](https://github.com/mukaschultze/grand-theft-auto-for-unity/issues/1).

## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE) for more information.