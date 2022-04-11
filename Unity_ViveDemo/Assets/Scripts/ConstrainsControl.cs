using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations.Rigging;


public class ConstrainsControl : MonoBehaviour
{
    [SerializeField]
    private GameObject avatarParent;
    private Transform eyeBoneR, eyeBoneL,headBone;
    [SerializeField]
    private MultiRotationConstraint eyeBoneL_Constraint, eyeBoneR_Constraint;
    [SerializeField]
    private GameObject headBone_Constraint;
    private string headBoneName;

    void Start()
    {
        headBoneName = "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 Neck/Bip01 Head";
        RigBuilder rigs = avatarParent.gameObject.AddComponent<RigBuilder>();
        Rig rig = transform.GetComponent<Rig>();
        rigs.layers.Add(new RigBuilder.RigLayer(rig,true));

        eyeBoneR = FindSetBone("/Bip01 REye");
        eyeBoneL = FindSetBone("/Bip01 LEye");
        headBone = FindSetBone("");

        SetEyeRotConstrainedObject(eyeBoneR_Constraint, eyeBoneR);
        SetEyeRotConstrainedObject(eyeBoneL_Constraint, eyeBoneL);

        MultiRotationConstraint headBone_RotConstraint = headBone_Constraint.GetComponent<MultiRotationConstraint>();
        MultiPositionConstraint headBone_PosConstraint = headBone_Constraint.GetComponent<MultiPositionConstraint>();

        SetHeadRotConstrainedSourceObject(headBone_RotConstraint);
        SetHeadPosConstrainedSourceObject(headBone_PosConstraint);

        rigs.Build();
    }

    Transform FindSetBone(String eyeBoneName)
    {
       Transform bone = avatarParent.transform.Find(headBoneName + eyeBoneName);
        return bone;

    }

    void SetEyeRotConstrainedObject(MultiRotationConstraint eyeBone_Constraint, Transform eyeBone)
    {
        Transform eyeBoneCon = eyeBone_Constraint.data.constrainedObject;
        eyeBoneCon = eyeBone.transform;
        eyeBone_Constraint.data.constrainedObject = eyeBoneCon;
    }

    void SetHeadRotConstrainedSourceObject(MultiRotationConstraint headBone_RotConstraint)
    {
        Transform headBoneRotCon = headBone_RotConstraint.data.constrainedObject;
        headBoneRotCon = headBone.transform;
        headBone_RotConstraint.data.constrainedObject = headBoneRotCon;

        var data = headBone_RotConstraint.data.sourceObjects;
        data.Clear();
        data.Add(new WeightedTransform(Camera.main.transform, 1));
        headBone_RotConstraint.data.sourceObjects = data;
    }
    void SetHeadPosConstrainedSourceObject(MultiPositionConstraint headBone_PosConstraint)
    {
        Transform headBonePosCon = headBone_PosConstraint.data.constrainedObject;
        headBonePosCon = headBone.transform;
        headBone_PosConstraint.data.constrainedObject = headBonePosCon;

        var data = headBone_PosConstraint.data.sourceObjects;
        data.Clear();
        data.Add(new WeightedTransform(Camera.main.transform, 1));
        headBone_PosConstraint.data.sourceObjects = data;
    }


}
