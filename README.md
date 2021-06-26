# KK_MainGameVR

This is a BepInEx plugin for Koikatsu that allows you to play the main game
(including the Maker) in VR. Currently only the standing (aka room-scale)
mode is supported.

This plugin is based on the [KoikatuVR](https://github.com/Ooetksh/KoikatuVR)
plugin developed by vrhth, KoikatsuVrThrowaway and Ooetksh. If you are migrating
from KoikatuVR, refer to the 'Migrating' section.

## Prerequisites

* Koikatu or Koikatsu Party
* BepInEx 5.4 or later
* SteamVR
* A VR headset supported by SteamVR
* VR controllers

## Installation

1. Make sure BepInEx 5 has been installed.
2. Download and extract the latest zip file from
  [releases](https://github.com/mosirnik/KK_MainGameVR/releases).
3. If your base game is Koikatsu Party (the Steam version), rename the extracted
  `Koikatu_Data` folder into `Koikatsu Party_Data` (note the spelling).
4. Copy two folders, `BepInEx` and `Koikatu_Data` (or the renamed one), to the
  Koikatsu installation folder. If this is done correctly, you should find a folder
  `BepInEx\plugins\KK_MainGameVR` directly under the game's root folder.
5. Modify `Koikatu_Data\globalgamemanagers` or `Koikatsu Party_Data/globalgamemanagers`:
    1. Rename the file to `globalgamemanagers.orig` (or whatever you want to call it).
    2. Open `globalgamemanagers.orig` with [UABE](https://github.com/DerPopo/UABE/releases).
    3. Select the row with the path ID "11" and the type "Build Settings", then click "Export Dump".
    4. Use a text editor to edit the generated dump file as shown below.
    5. Click "Import Dump" to load the edited dump file.
    6. Click "OK" to save the modified file as `globalgamemanagers`, so that it
      replaces the file you renamed in the first step.
    ~~~
    Before:
    0 vector enabledVRDevices
    0 Array Array (0 items)
    0 int size = 0

    After:
    0 vector enabledVRDevices
    0 Array Array (2 items)
    0 int size = 2
    [0]
     1 string data = "None"
    [1]
     1 string data = "OpenVR"
    ~~~

Now you can start Koikatsu with `--vr` command line option to enable VR.
Alternatively, starting Koikatsu while SteamVR is running also enables this
plugin.

## Control

This plugin assumes that your VR controller has the following buttons/controls:

* Application menu button
* Trigger button
* Grip button
* Touchpad

You may need to tweak button assignments in SteamVR's per-game settings if your
controllers don't natively have these. If you are using Windows MR controllers,
you may also need to configure them to pretend to be Vive controllers, for
reasons I don't understand.

In the game, each of the controllers has 3 tools: Menu, Warp and School. Only
one of them can be active at a time. You can cycle through the tools by pressing
the Application menu button. Each controller has a cyan icon indicating which
tool is currently active.

### Menu tool <img src="https://raw.githubusercontent.com/mosirnik/KK_MainGameVR/master/doc/img/icon_menu.png" height="30">

The menu tool comes with a little screen on which game menus, icons and texts
are shown. You can use the other controller as a virtual laser pointer, and
pull the Trigger to click on the screen. Most game interactions (specifically,
the ones that don't involve touching 3D objects) are done this way.

Pressing the Grip button while the Menu tool is active causes the screen
to be detached and left at the current position in the 3D space. Pressing it
again reclaims the screen.

A detached screen can be resized and moved around by using two controllers:
point both controllers at the screen, hold the triggers and then move them
around.

### Warp tool <img src="https://raw.githubusercontent.com/mosirnik/KK_MainGameVR/master/doc/img/icon_warp.png" height="30">

The warp tool allows you to move around in the 3D space. 

Use the touchpad to teleport. Holding the Grip button allows you to grab
the world and move it around. While doing so, you can hold the Grip of the
other controller to start rotating.

### School tool <img src="https://raw.githubusercontent.com/mosirnik/KK_MainGameVR/master/doc/img/icon_school.png" height="30">

This tool is a collection of Koikatsu-specific action commands and simulated
mouse/keyboard inputs. There are two button mappings, one for H scenes and
one for all other scenes. Both mappings are configurable. The defaults for
non-H scenes are:

* Trigger: Walk (Roam mode)
* Grip: Middle mouse button
* Touchpad up: F3
* Touchpad down: Move protagonist to camera (Roam mode)
* Touchpad left: Rotate left (Roam mode)
* Touchpad right: Rotate right (Roam mode)
* Touchpad center: Right mouse button

For H scenes:

* Trigger: Left mouse button
* Grip: Middle mouse button
* Touchpad up: Mouse wheel scroll up
* Touchpad down: Mouse wheel scroll down
* Touchpad left: (unassigned)
* Touchpad right: (unassigned)
* Touchpad center: Right mouse button

## Situation-specific controls

The school tool can be used when you need more complex interactions than simple
mouse clicks.

There are also a few types of context-specific controls, where you can interact
directly with 3D objects using the controllers. This type of interaction does
not require any specific tool to be selected. The tool icon disappears when
such an interaction is available.

Below is a list of situations that offer special controls.

### Roaming

In the Roaming mode, there are 2 main methods of moving around:

* Use the school tool to walk (default: Trigger), and turn left and right
    (default: Touchpad left and right). In this method, your point
    of view is fixed at the protagonist's head.
* Use the warp tool to move your viewpoint, then use the school tool
    (default: Touchpad down) to summon the protagnoist.

Either way, you can use the school tool to simulate ordinary mouse and keyboard
inputs, e.g. right click (default: Trigger) for interacting with an object,
middle click (default: Grip) for opening the menu, etc.

### Talk scene

When talking to a character, most interactions are done through the menu.
In addition, you can touch or look at the character by putting one of the
controllers at the position you want to touch/look at, then pulling the
Trigger. 

### H scene

Caressing can be done in the same way as touching in talk scenes. Additionally,
you can switch to a different mode of caressing by pressing the Application
menu button with the controller in place.

Optionally, automatic touching and kissing can be enabled, so that you don't
even need to pull the Trigger.

When changing location, you can use the green laser to point to a new location
icon and pull the Trigger to confirm.

## Configuration

It is recommended that you use
[ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager),
which allows you to change settings of this plugin from within the game.

Alternatively you can manually edit `BepInEx\config\mosirnik.kk-main-game-vr.cfg`
with a text editor.

## Migrating

Major differences between this plugin and Ooetksh's version of KoikatuVR include:

* It is a BepInEx 5 plugin.
* It adds a few ways of interacting with 3D objects using conrollers, like
  touching a character or changing location in H.
* It no longer reads `VRContext.xml` or `VRSettings.xml`. It uses BepInEx-style
  configuration instead.
* All keyboard shortcuts and the seated mode have been removed.

If you are migrating from KoikatuVR, make sure to remove or disable KoikatuVR
before installing this plugin, except that you can keep the modified version
of the `globalgamemanagers` file.
