
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// TODO: Figure out how we're going to figure out what the sources of data are actually from. 
/// Currently, I think this will be done by looking at the array length
/// </summary>
public class ZeroMQRelay : MonoBehaviour 
{

    public static Action<AnimationDataFrame> OpenFaceDataReceived;

    public static string[] OpenFaceDataColumns =
    {
    "timestamp",
    "head_pos_x",
    "head_pos_y",
    "head_pos_z",
    "head_x",
    "head_y",
    "head_z",
    "left_eye_x",
    "left_eye_y",
    "left_eye_z",
    "right_eye_x",
    "right_eye_y",
    "right_eye_z",
    "AU01_InnerBrowRaiser",
    "AU02_OuterBrowRaiser",
    "AU04_BrowLowerer",
    "AU05_UpperLidRaiser",
    "AU06_CheekRaiser",
    "AU07_LidTightener",
    "AU09_NoseWrinkler",
    "AU10_UpperLipRaiser",
    "AU12_LipCornerPuller",
    "AU14_Dimpler",
    "AU15_LipCornerDepressor",
    "AU17_ChinRaiser",
    "AU20_LipStretcher",
    "AU22_LipTightener",
    "AU25_LipsPart",
    "AU26_JawDrop",
    "AU45_Blink"
    };

    private void OnEnable()
    {

        ZeroMQReceiver.NewZeroMQMessageReceivedEvent += OnNewMessageReceived;
    }

    private void OnDisable()
    {
        ZeroMQReceiver.NewZeroMQMessageReceivedEvent -= OnNewMessageReceived;
    }

    private void OnNewMessageReceived(List<string> messages)
    {
        AnimationDataFrame frame;
        foreach (string message in messages)
        {
            
            frame = DeserializeString(message);
            if (frame.d.Length == (OpenFaceDataColumns.Length - 1))
            {
               // Debug.Log(frame);
                OpenFaceDataReceived?.Invoke(frame);
            }
        }
    }

    private AnimationDataFrame DeserializeString(string message)
    {
        var unescaped = Regex.Unescape(message);
        AnimationDataFrame paf = JsonConvert.DeserializeObject<AnimationDataFrame>(unescaped);
        return paf;
    }
}

[Serializable]
public struct AnimationDataFrame
{
    public float t;
    public float[] d;
}


