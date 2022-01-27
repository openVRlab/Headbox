# Headbox

HeadBox is a series of opensource tools to do facial animation on the Microsoft Rocketbox avatar library. It includes a tool to create blendshapes out of the facial bones inside Maya and transfer the new blendshapes to the other avatars in the library. We have created a total of 15 visemes, 48 FACS, 30 for the Vive facial tracker. These blendshapes have been released with the original library. An additional Unity demo shows the use these tools with Openface and Oculus Lipsync.

In this repo you will find a Unity Demo of the Headbox tool for blendshape creation to do facial animation on the Microsoft Rocketbox.
And the Python script for Maya to create new blendshapes.

<img src="/Documentation/F96C911C-D2D8-49B1-BCB8-45C582CFD945.jpeg" alt="Headbox Scheme" title="Headbox Tools"/>

https://www.youtube.com/watch?v=hgUGOjc6hOg

This repo is part of 
Volonte M, Ofek E, Jakubzak K, Bruner S, and Gonzalez-Franco M (2022) HeadBox: A Facial Blendshape Animation Toolkit for the Microsoft Rocketbox Library. IEEE VR 2022

Presented in the Open Access VR tools and libraries Workshop

This unity demo has dependencies from Openface and Oculus Lipsync therefore we can't provide any particular license and it has to be looked up to the original sources.

## Openface
OpenFace is a state-of-the art tool intended for facial landmark detection, head pose estimation, facial action unit recognition, and eye-gaze estimation.

Openface is only included as a release (no source code) with a direct output via ZeroMQ to the unity project.

Original Project and Source Code:
https://github.com/TadasBaltrusaitis/OpenFace

There are two files from openface training that are too big for github

*cen_patches_0.50_of.dat
*cen_patches_1.00_of.dat

please find them here

wget https://www.dropbox.com/sh/o8g1530jle17spa/AACdS_lkcAhwDghZVq3MgMcza/cen_patches_0.50_of.dat
wget https://www.dropbox.com/sh/o8g1530jle17spa/AABis9pvPp-cKI10u6McOL8-a/cen_patches_1.00_of.dat

and place them in OpenFaceRelease\Release\model\patch_experts


Openface license states: ACADEMIC OR NON-PROFIT ORGANIZATION NONCOMMERCIAL RESEARCH USE ONLY
Please check the license in the repo for more details on usage.


## Oculus Lipsync

Oculus Lipsync is an add-on plugin and set of scripts which can be used to sync mouth movements of a game or other digital asset to speech sounds from pre-recorded audio or live microphone input in real-time.

Latest version and documentation is available here
https://developer.oculus.com/downloads/package/oculus-lipsync-unity/

This part of the Unity demo needs to be used according to the Oculus SDK License Agreement https://developer.oculus.com/licenses/oculussdk/


## Using the demo

Lipsyc
In all the Microsoft Rocketbox Avatars the first 15 blenshapes are the visemes compatible with Oculus Lipsync, which can be defined in the OculusLipSync object in the Hierarchy.
If you change the target avatar then you need to modify the Skinned Mesh renderer inside the OVR Lip Synch Context Morph Target component.
 
<img src="/Documentation/OculusLipsync.JPG" alt="Blendshape mapping file" title="Blendshape mapping file"/>

Facial Tracking 


The Headbox_Openface object in the Hierarchy contains the ZeroMQ receiver to retrieve the data from the Openface Executable and the FaceAnimator component that has the Blendshape Mapping File. This file will assign the Action Units from Openface to the blendshapes in the avatar. You need to set the avatar and head of the target avatar in this component.
<img src="/Documentation/openface.JPG" alt="Blendshape mapping file" title="Blendshape mapping file"/>


The mapping file is a json file where one can set a maximum on the threshold or weight of the blenshapes to tune a bit the effects on the animation.
 
<img src="/Documentation/mapping.JPG" alt="Blendshape mapping file" title="Blendshape mapping file"/>

## Creating new blendshapes
You can use the Maya python script to move the bones on one avatar of the library and export the blendshape across all the other avatars.
