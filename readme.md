# MeidoPhotoStudio

A MultipleMaids alternative

## Features

* Scene manager akin to the one in my fork of [ModifiedMM](https://git.coder.horse/habeebweeb/modifiedMM)
* Attach ANY prop that MeidoPhotoStudio can spawn
* Pose and hand preset categorization
* Support for studio mode hand preset saving and loading
* Revamped light management
* Many quality of life improvements
* Lightweight (1/5 of the size of MultipleMaids)

## Screenshots

*Scene manager*  
![Scene manager screenshot](./img/scene_manager.jpg)

*Attach props*  
![Attach prop screenshot](./img/attach_prop.jpg)

*More clothing toggles*  
![Clothing toggles screenshot](./img/more_clothing_toggles.jpg)

*Custom pose categories*  
![Custom pose screenshot](./img/custom_pose.jpg)

*Custom hand presets and categories*  
![Custom pose screenshot](./img/hand_presets.jpg)

## Why Though?

![MM is spaghetti code](./img/spaghetti_code.jpg)

Despite being an overall good plugin, MultipleMaids (MM) is plagued with many bugs and is seldom maintained by the original developers.

ModifiedMM was an effort to fix some of the problems with MM and even add new features. Although great progress was made, MM's code base prevents anyone, but the original developer (hopefully), from making any meaningful changes. The code base can rival that of the flying spaghetti monster.

As well as MM being hard to maintain, ModifiedMM was created against the wishes of the original developers.

> "転載・配布・改変したプラグインの公開は禁止します。" (使い方.txt)  
> "Publication of reprinted/distributed/modified plug-ins is prohibited" (Google Translate).

MeidoPhotoStudio is an attempt at making MultipleMaids truly great again.

## Building

### Libraries for MeidoPhotoStudio

These libraries (aside from BepInEx) are all found in COM3D2's Managed folder

Place these in a folder called `lib`

* `Assembly-CSharp.dll`
* `Assembly-CSharp-firstpass.dll`
* `Assembly-UnityScript-firstpass.dll`
* `Newtonsoft.json.dll`
* `UnityEngine.dll`
* `Ionic.Zlib.dll`
* [`BepInEx.dll`](https://github.com/BepInEx/BepInEx/releases) from `BepInEx\core`
