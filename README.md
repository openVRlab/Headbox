# Headbox

HeadBox is a series of opensource tools to do facial animation on the Microsoft Rocketbox avatar library (available here https://github.com/microsoft/Microsoft-Rocketbox). It includes a tool to create blendshapes out of the facial bones inside Maya and transfer the new blendshapes to the other avatars in the library. We have created a total of 15 visemes, 48 FACS, 30 for the Vive facial tracker and 52 ARKit blendshapes. These blendshapes have been released with the original library. An additional Unity demo shows the use these tools with Openface and Oculus Lipsync.

In this repo you will find a Unity Demo of the Headbox tool for blendshape creation to do facial animation on the Microsoft Rocketbox.
And the Python script for Maya to create new blendshapes.

<img src="/Documentation/F96C911C-D2D8-49B1-BCB8-45C582CFD945.jpeg" alt="Headbox Scheme" title="Headbox Tools"/>

https://www.youtube.com/watch?v=hgUGOjc6hOg

## Reference
The following paper was published at IEEE VR 2022 to coincide with the release of this toolbox, and gives more details of the features included:

Volonte M, Ofek E, Jakubzak K, Bruner S, and Gonzalez-Franco M (2022) [HeadBox: A Facial Blendshape Animation Toolkit for the Microsoft Rocketbox Library](https://www.microsoft.com/en-us/research/publication/headbox-a-facial-blendshape-animation-toolkit-for-the-microsoft-rocketbox-library/). IEEE VR 2022

Presented in the Open Access VR tools and libraries Workshop. If you use this library for research or academic purposes, please also cite the aforementioned paper.

This unity demo has dependencies from Openface and Oculus Lipsync therefore we can't provide any particular license and it has to be looked up to the original sources.

## Openface
OpenFace is a state-of-the art tool intended for facial landmark detection, head pose estimation, facial action unit recognition, and eye-gaze estimation.

Openface is only included as a release (no source code) with a direct output via ZeroMQ to the unity project.

Original Project and Source Code:
https://github.com/TadasBaltrusaitis/OpenFace

<!--- There are two files from openface training that are too big for github

*cen_patches_0.50_of.dat
*cen_patches_1.00_of.dat

please find them here

wget https://www.dropbox.com/sh/o8g1530jle17spa/AACdS_lkcAhwDghZVq3MgMcza/cen_patches_0.50_of.dat
wget https://www.dropbox.com/sh/o8g1530jle17spa/AABis9pvPp-cKI10u6McOL8-a/cen_patches_1.00_of.dat

and place them in OpenFaceRelease\Release\model\patch_experts
 ---> 

Openface license states: ACADEMIC OR NON-PROFIT ORGANIZATION NONCOMMERCIAL RESEARCH USE ONLY
Please check the license in the repo for more details on usage.


## Oculus Lipsync

Oculus Lipsync is an add-on plugin and set of scripts which can be used to sync mouth movements of a game or other digital asset to speech sounds from pre-recorded audio or live microphone input in real-time.

Latest version and documentation is available here
https://developer.oculus.com/downloads/package/oculus-lipsync-unity/

This part of the Unity demo needs to be used according to the Oculus SDK License Agreement https://developer.oculus.com/licenses/oculussdk/


## Using the demo

<img src="/Documentation/F31594C2-4BB4-49B4-8738-D95C5780EB5B.jpeg" alt="Blendshape mapping file" title="Blendshape mapping file"/>

### Lipsyc
In all the Microsoft Rocketbox Avatars the first 15 blenshapes are the visemes compatible with Oculus Lipsync, which can be defined in the OculusLipSync object in the Hierarchy.
If you change the target avatar then you need to modify the Skinned Mesh renderer inside the OVR Lip Synch Context Morph Target component.
 
<img src="/Documentation/OculusLipsync.JPG" alt="Blendshape mapping file" title="Blendshape mapping file"/>


### Facial Tracking 

The Headbox_Openface object in the Hierarchy contains the ZeroMQ receiver to retrieve the data from the Openface Executable and the FaceAnimator component that has the Blendshape Mapping File. This file will assign the Action Units from Openface to the blendshapes in the avatar. You need to set the avatar and head of the target avatar in this component.
<img src="/Documentation/openface.JPG" alt="Blendshape mapping file" title="Blendshape mapping file"/>


The mapping file is a json file where one can set a maximum on the threshold or weight of the blenshapes to tune a bit the effects on the animation.
 
<img src="/Documentation/mapping.JPG" alt="Blendshape mapping file" title="Blendshape mapping file"/>

### VIVE Unity Demo Project

#### Get Started with VIVE

Before opening the Unity demo project, please set up your VIVE devices and Launch SRanipal Runtime first:

+ VIVE Pro Eye HMD and Lip Tracker Installation

Set up the lighthouse base stations and headset like a normal VIVE Pro and make sure the lip tracker is plugged in the headset. A setup guide for VIVE Pro Eye HMD can be found here https://www.vive.com/us/setup/vive-pro-hmd/

+ Install and Run SRanipal Runtime

SRanipal Runtime(SR_Runtime) can be downloaded from the VIVE developer portal. Launch SR_Runtime until the status icon appears in the notification tray. 

There are 3 status modes for launched SR_Runtime：
① Black: HMD does not support face tracking.   ② Orange: The face tracking device is in idle mode.   ③ Green: Face tracking is active.
<p align="center">
<img src="/Documentation/SRanipal Runtime Icon modes.png" alt="SRanipal Runtime Icon Modes" title="SRanipal Runtime Icon Modes"/>
</p>

#### Run Unity Demo

+ Steam VR Plugin

This Demo’s VR setting is based on Steam VR, please make sure your Steam VR is working and  OpenVR Loader is selected in the project setting. Then, Steam VR should start automatically when you hit play.
<p align="center">
<img src="/Documentation/XR plugin setting.png" alt="VR Setting" title="VR Setting"/>
</p>

**Note**: If the project is reporting errors, it is most likely because you need to reimport Steam VR Plugin into your local Unity project via asset store https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647.

+ VIVE Eye and Facial Tracking SDK

The “Vive-SRanipal-Unity-Plugin.unitypackage” in VIVE Eye and Facial Tracking SDK has been imported and modified in this demo project. 

According to the blendshape document provided by VIVE, all the Microsoft Rocketbox Avatars include 42 blendshapes (SR_01 to SR_42). More information about supported blendshape and API can be found in VIVE official SDK, which can be downloaded from https://developer-express.vive.com/resources/vive-sense/eye-and-facial-tracking-sdk/.

+ Headbox_ViveDemo Sample Scene

There are two main sample scripts (SRanipal_AvatarEyeSample_v2, SRanipal_AvatarLipSample_v2) that control the facial tracking in the sample scene. If you need to use other avatars rather than the sample avatar, you need to set the eye and lip tracking in these two components by following the next steps to assign eyes and lip blendshapes: 

1. Add Constraints to Avatar
You can find a prefab called “ConstraintsControl” and put them as a child game object under the avatar you used in the Hierarchy. 
<p align="center">
<img src="/Documentation/Constraint.png" alt="Constraint prefab" title="Constraint prefab"/>
</p>

2. Assign Eye Model Constraints
Then, assign the “EyeModel_L_Constraint” and “EyeModel_R_Constraint” in the prefab to the eye sample script. 
<p align="center">
<img src="/Documentation/Eye Model.png" alt="CAssign Eye Model Constraints" title="Assign Eye Model Constraints"/>
</p>

3. Assign Eyes and Lip Blendshape
The corresponding VIVE  blendShape would be automatically linked when assigning the avatar’s skinned mesh renderer to the eye and lip shape tables.
<p align="center">
<img src="/Documentation/Eye blendshape.png" alt="Assign Eyes Blendshape" title="Assign Eyes Blendshape"/>
<img src="/Documentation/Lip blendshape.png" alt="Assign Lip Blendshape" title="Assign Lip Blendshape"/>
</p>

**Note**: The mapped avatar’s eye and lip blendshape can be changed with ‘avatarName’ and ‘headboxWeight’ value as below, if you need to use other avatars with different blendshape names.

``` c#
public class SRanipal_AvatarLipSample_v2 : MonoBehaviour
{
	public const string BLENDSHAPE_PREFIX = "blendShape1.SR_";

	//Change the headboxWeight below
	public static List<LipMapping> LipMappings = new List<LipMapping> 
            {
		new LipMapping{viveName = LipShape_v2.Cheek_Puff_Left, avatarName = $"{BLENDSHAPE_PREFIX}01_Cheek_Puff_Left", headboxWeight = 1.0f},
		new LipMapping{viveName = LipShape_v2.Cheek_Puff_Right, avatarName = $"{BLENDSHAPE_PREFIX}02_Cheek_Puff_Right",headboxWeight = 1.0f},
		new LipMapping{viveName = LipShape_v2.Cheek_Suck, avatarName = $"{BLENDSHAPE_PREFIX}03_Cheek_Suck",headboxWeight = 0.5f}
            }
}

```

### ARKit Unity Demo Project
#### Get Started with VIVE
+ Live Capture 

The facial tracking in this demo requires iPad or iPhone support, please install the "Unity Face Capture" application from Apple Store. The Unity Face Capture is part of the Live Capture package provided by Unity, you can find more information about the Live Capture package from https://docs.unity3d.com/Packages/com.unity.live-capture@1.0/manual/index.html.

+ Apple blendShapes

Each avatar in Rocketbox Avatar Library includes 52 ARKit blendshape (AK_01 to AK_52), you can find more information about these blendshapes in Apple Developer Documentation here https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation. 

#### Run Unity Demo

+ ARKit Face Actor and Face Mapper

An ARKit Face Actor is assigned to the sample avatar as a component, and also to the "ARKit Face Device" component on FaceDevice in Hierarchy(Take Recorder/FaceDevice). 

The Face Mapper can be created via Live Capture in the project. A sample Headbox Face Mapper is assigned to "ARKit Face Actor" component, which you can find in the folder “Assets/ARKit Sample/Face Capture/FaceMapper”. 

<img src="/Documentation/ARkitDemo.png" alt="ARkit Demo with Connections and ARKit Face Actor" title="ARkit Demo"/>

+ Avatar Head Constraint

A head Constraint prefab can be found in Assets, if you need to change to other avatars in Rocketbox, you need to add the Constraint prefab as avatar's child game object and assign it to Face Mapper's Head Rotation.

+ Live Capture Connection

Once Live Capture set up correctly, open the Connection from “Window/Live Capture/Connection”, and make sure your iPhone/iPad is on the same Wifi as your computer. If it is the first time you used connection, you need to Create Server first. Then play the scene and at the same time, open Unity Face Capture, you will find you can both manually set the port and IP for servers to connect Unity projects, or choose automatically scan by the the Unity Face Capture on your iPhone/iPad. Then, you can control the facial tracking via Apple device and record animation through the Take Recorder in the Inspector.


## Creating new blendshapes
You can use the Maya python script to move the bones on one avatar of the library and export the blendshape across all the other avatars.


# Main Contributors

Mar Gonzalez-Franco - Microsoft Research

Eyal Ofek - Microsoft Research

### MAYA tool and blendshapes
Matias Volonte - Northeastern University & Clemenson University

### Demo with HTC VIVE Facial Tracker and AR Kit compatibility
Xueni Pan - Goldsmiths

Fang Ma - Goldsmiths

### OpenFace and ZeroMQ connection
Ken Jakubzak - Microsoft

## Contributing
This project welcomes contributions and suggestions. 
