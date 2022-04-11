using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations.Rigging;

public class ConstraintControl : MonoBehaviour
{
    [SerializeField]
    private GameObject avatarParent;
    private Transform headBone;
    [SerializeField]
    private GameObject headBone_Constraint;
    private string headBoneName;

    void Start()
    {
        headBoneName = "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 Neck/Bip01 Head";
        
        RigBuilder rigs = avatarParent.gameObject.AddComponent<RigBuilder>();
        Rig rig = transform.GetComponent<Rig>();
        rigs.layers.Add(new RigLayer(rig, true));

        headBone = FindSetBone("");

        MultiRotationConstraint headBone_RotConstraint = headBone_Constraint.GetComponent<MultiRotationConstraint>();
        SetHeadRotConstrainedObject(headBone_RotConstraint);

        rigs.Build();
    }

    Transform FindSetBone(String eyeBoneName)
    {
        Transform bone = avatarParent.transform.Find(headBoneName + eyeBoneName);
        return bone;

    }

    void SetHeadRotConstrainedObject(MultiRotationConstraint headBone_RotConstraint)
    {
        Transform headBoneRotCon = headBone_RotConstraint.data.constrainedObject;
        headBoneRotCon = headBone.transform;
        headBone_RotConstraint.data.constrainedObject = headBoneRotCon;
    }
}
