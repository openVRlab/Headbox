using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FaceAnimator : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;

    public GameObject avatar, head_bone;
    private Mesh skinnedMesh;

    private Dictionary<string, int> blendDictStringToInt = new Dictionary<string, int>();
    private Dictionary<int, string> blendDictIntToString = new Dictionary<int, string>();
    private HashSet<int> overallBlendshapes;

    public float speakingThreshold = 10f;
    public float blendshapeInterpolationSpeed = 10f;
    public float headRotationSpeed = 10f;
    public float eyeRotationSpeed = 10f;

    public float headRotationMultiplier = .1f;
    int blendShapeCount = 0;

    private AnimationDataFrame frameData;

    private bool hasNewDataUpdate = false;
    private bool hasNewBlendshapeVals  = false;

    public InterpolationType BlendshapeInterpolationType = InterpolationType.EaseInOut;

    private Dictionary<int, float> curFrameBlendshapeVals = new Dictionary<int, float>();
    private Dictionary<int, float> targetFrameBlendshapeVals = new Dictionary<int, float>();

    [Tooltip("The JSON file that contains the input-to-blendshape blendshape mapping.")]
    public TextAsset BlendshapeMappingFile;

    public SerializedBlendshapeMapping[] mappedBlendshapes; 
    void Awake()
    {
        // get MB / MH model
        skinnedMeshRenderer = avatar.GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = avatar.GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    // Use this for initialization
    void Start()
    {
        // create dict of all blendshapes this skinnedMesh has

        Debug.Log("Loading Blendshapes");
        blendShapeCount = skinnedMesh.blendShapeCount;
        for (int i = 0; i < blendShapeCount; i++)
        {
            string expression = skinnedMesh.GetBlendShapeName(i);
            blendDictStringToInt.Add(expression, i);
            blendDictIntToString.Add(i, expression);
        }

        var serializedMapping = JsonUtility.FromJson<SerializedBlendshapeMappingData>(BlendshapeMappingFile.ToString());
        mappedBlendshapes = serializedMapping.data;
        
        overallBlendshapes = new HashSet<int>();
        HookIntoZeroMQRelay();
    }

    private void Update()
    {
        if (hasNewDataUpdate)
        {            
            curFrameBlendshapeVals = new Dictionary<int, float>();
            // Deal with the timestamps, head + eye positions and rots, etc.

            Vector3 headRot = new Vector3(frameData.d[3] * Mathf.Rad2Deg, // Represents Up/down in OpenFace
                               frameData.d[4] * Mathf.Rad2Deg, // Turn
                               frameData.d[5] * Mathf.Rad2Deg); // Tilt
            // Convert world corrdinate of `headRot` into local one, using `head_bone`
            Vector3 headRotLocal = head_bone.transform.InverseTransformDirection(headRot);
            head_bone.transform.localRotation = Quaternion.Slerp(head_bone.transform.localRotation,
                Quaternion.Euler(headRotLocal),
                Time.deltaTime * headRotationSpeed);


            foreach (var blend in overallBlendshapes)
            {
                curFrameBlendshapeVals[blend] = 0f;
            }

            // Take care of the blendshape frames
            for (int i = 12; i < frameData.d.Length; i++)
            {
                var mapping = mappedBlendshapes[i - 12];
                if (frameData.d[i] > 0)
                {
                    foreach (var blendshape in mapping.weightedBlendshapes)
                    {
                        float val = ((frameData.d[i] / 5.0f) * 100f) * blendshape.weight;
                        if (val >= mapping.threshold)
                        {
                            if (curFrameBlendshapeVals.ContainsKey(blendDictStringToInt[blendshape.targetBlendshape]))
                            {
                                curFrameBlendshapeVals[blendDictStringToInt[blendshape.targetBlendshape]] += val;
                            }
                            else
                            {
                                curFrameBlendshapeVals[blendDictStringToInt[blendshape.targetBlendshape]] = val;
                            }
                        }
                        else
                        {
                            curFrameBlendshapeVals[blendDictStringToInt[blendshape.targetBlendshape]] = 0f;
                        }

                        if (!overallBlendshapes.Contains(blendDictStringToInt[blendshape.targetBlendshape]))
                        {
                            overallBlendshapes.Add(blendDictStringToInt[blendshape.targetBlendshape]);
                        }
                    }
                }
            }

            hasNewDataUpdate = false;
            hasNewBlendshapeVals = true;
        }
    }

    public void LateUpdate()
    {
        if (hasNewDataUpdate)
        {
            targetFrameBlendshapeVals.Clear();
            foreach (var curFrameBlendShape in curFrameBlendshapeVals)
            {
                targetFrameBlendshapeVals.Add(curFrameBlendShape.Key, curFrameBlendShape.Value);
            }

            hasNewBlendshapeVals = false;
        }

        foreach (var curFrameBlendShape in curFrameBlendshapeVals)
        {
            float blendshapeVal = Interpolate(BlendshapeInterpolationType, skinnedMeshRenderer.GetBlendShapeWeight(curFrameBlendShape.Key), curFrameBlendShape.Value, Time.deltaTime * blendshapeInterpolationSpeed);
            skinnedMeshRenderer.SetBlendShapeWeight(curFrameBlendShape.Key, blendshapeVal);
        }
    }

    public void OnDataUpdated(AnimationDataFrame lastDataSet)
    {
        frameData = lastDataSet;
        hasNewDataUpdate = true;
    }

    public void HookIntoZeroMQRelay()
    {
        ZeroMQRelay.OpenFaceDataReceived += OnDataUpdated;
    }

    public void UnhookFromZeroMQRelay()
    {
        ZeroMQRelay.OpenFaceDataReceived -= OnDataUpdated;
    }

    private bool IsCurrentlySpeaking()
    {
       /* foreach (var viseme in LipSyncComponent.MappedVisemes)
        {
            foreach (var blendshape in viseme.WeightedVisemes)
            {
                if (skinnedMeshRenderer.GetBlendShapeWeight(blendDictStringToInt[blendshape.targetBlendshape]) > speakingThreshold)
                {
                    return true;
                }
            }
        }*/

        //foreach (var blendshape in LipSyncComponent.LaughterBlendTargets)
        //{
        //    if (skinnedMeshRenderer.GetBlendShapeWeight(blendDictStringToInt[blendshape]) > speakingThreshold)
        //    {
        //        return true;
        //    }
        //}

        return false;
    }
    public enum InterpolationType
    {
        Linear = 0,
        EaseIn = 1,
        EaseOut = 2,
        EaseInOut = 3,
        Boing = 4,
        Hermite = 5,
    }

    /// <summary>
    /// Interpolates from a value to another value using a specified interpolation type.
    /// </summary>
    /// <param name="type">The type of interpolation to use. EaseInOut is recommended for most cases.</param>
    /// <param name="from">The value at the start of interpolation.</param>
    /// <param name="to">The value at the end of interpolation.</param>
    /// <param name="t">A value from 0 to 1 that determines how far along the interpolation is.</param>
    /// <returns>The interpolated value calculated from the input parameters.</returns>
    public static float Interpolate(InterpolationType type, float from, float to, float t)
    {
        switch (type)
        {
            case InterpolationType.Linear:
                return Mathf.Lerp(from, to, t);
            case InterpolationType.EaseIn:
                return Mathf.Lerp(from, to, 1.0f - Mathf.Cos(t * Mathf.PI * 0.5f));
            case InterpolationType.EaseOut:
                return Mathf.Lerp(from, to, Mathf.Sin(t * Mathf.PI * 0.5f));
            case InterpolationType.EaseInOut:
                return Mathf.SmoothStep(from, to, t);
            case InterpolationType.Boing:
                t = Mathf.Clamp01(t);
                t = (Mathf.Sin(t * Mathf.PI * (0.2f + 2.5f * t * t * t)) * Mathf.Pow(1f - t, 2.2f) + t) * (1f + (1.2f * (1f - t)));
                return from + (to - from) * t;
            case InterpolationType.Hermite:
                return Mathf.Lerp(from, to, t * t * (3.0f - 2.0f * t));
            default:
                Debug.LogError("Invalid InterpolationType of: " + type.ToString());
                return from;
        }
    }
}

[Serializable]
public class AffectedBlendshape
{
    public string targetBlendshape;
    public float weight;
}

[Serializable]
public class SerializedBlendshapeMapping
{
    public string inputName;
    public AffectedBlendshape[] weightedBlendshapes;
    public float threshold;
}

[Serializable]
public class SerializedBlendshapeMappingData
{
    public SerializedBlendshapeMapping[] data;
}


[Serializable]
public class SerializedVisemeMapping
{
    public string inputName;
    public AffectedBlendshape[] WeightedVisemes;
    public float threshold;
}

[Serializable]
public class SerializedVisemeMappingData
{
    public SerializedVisemeMapping[] data;
}
