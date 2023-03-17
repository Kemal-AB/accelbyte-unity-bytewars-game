# Byte Wars - Unreal Engine

## Overview

Byte Wars is the official tutorial game for AccelByte Gaming Services (AGS). It is intended to act as a sample project that can be used as a reference on the best practices to integrate our services into your game. We created Byte Wars from scratch as a fully functional offline game. This offline game was then brought online with the power of AccelByte’s platform by adding different services from each of our service areas like access, play and engagement. Every tutorial module walks you through a step by step guide to add a specific feature to Byte Wars which you can then translate into your own game.

## Prerequisites

* Use **Unity Editor** 2021 LTS with minimum version 2021.3.16f1.


## Clone Byte Wars

This repository has two main branches, master and online.
* **Main branch** is the vanila version game source code without any submodule.
* **Online branch** is the branch that has the AccelByte's Plugins, which are used by the Byte Wars Tutorial Module 1.
* **tutorial/online-module.2** is the branch based on `online` branch and also has the required resources for user to learn Byte Wars Tutorial Module 2.

### Main Branch

Just run the following git command to clone the game.
```batch
git clone git@github.com:AccelByte/accelbyte-unity-bytewars-game.git
```
### Online and Tutorial Module Branches

The online branch and the tutorial module 2 branch has a package of AccelByte Unity SDK setup in package manager from the following GitHub link:
* [AccelByte Unity SDK](https://github.com/AccelByte/accelbyte-unity-sdk).

This AccelByte Unity SDK package are required to follow along the Byte Wars Tutorial and it will be automatically downloaded when the project opened for the first time. 

## Open Byte Wars in Unity

1. From Unity Hub, go to Projects sidebar and click open in the project panel 
2. A file browser will appear, then select Byte Wars project folder and click open. 
3. The Byte Wars project will be added into the project panel with the editor version. Click on the project to open it in the Unity Editor.

## Run Byte Wars (from Main branch)

### Run via Editor

1. Open the Byte Wars project on your Unity Editor. 
2. From project window, go to Assets>Scenes then double click MainMenu scene file. Make sure MainMenu has opened in the hierarchy.

### Run via Packaged Game Client

1. Open the Byte Wars project on your Unity Editor 
2. In your menu taskbar, go to File > Build Settings, a popup window will appear and and leave build setting as default and click build button.
3. Select the destination folder to save the package file and press select folder button.
4. Open the destination folder and double click the ByteWars.exe to play the game.