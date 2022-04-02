using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;


public class ConstrainsControl : MonoBehaviour
{
    public GameObject avatarHeadBone;
    private Transform eyeBoneR, eyeBoneL,headBone;
    public MultiRotationConstraint eyeBoneL_Constraint, eyeBoneR_Constraint;
    public GameObject headBone_Constraint;
    //public MultiRotationConstraintData d;
    // Start is called before the first frame update
    void Start()
    {
        RigBuilder rigs = avatarHeadBone.gameObject.AddComponent<RigBuilder>();
        Rig rig = transform.GetComponent<Rig>();
        rigs.layers.Add(new RigBuilder.RigLayer(rig,true));

        eyeBoneR = avatarHeadBone.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 Neck/Bip01 Head/Bip01 REye");
        eyeBoneL = avatarHeadBone.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 Neck/Bip01 Head/Bip01 LEye");
        headBone = avatarHeadBone.transform.Find("Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 Neck/Bip01 Head");
        
        Transform eyeBoneRCon = eyeBoneR_Constraint.data.constrainedObject;
        eyeBoneRCon = eyeBoneR.transform;
        eyeBoneR_Constraint.data.constrainedObject = eyeBoneRCon;

        Transform eyeBoneLCon = eyeBoneL_Constraint.data.constrainedObject;
        eyeBoneLCon = eyeBoneL.transform;
        eyeBoneL_Constraint.data.constrainedObject = eyeBoneLCon;

        MultiRotationConstraint headBone_RotConstraint = headBone_Constraint.GetComponent<MultiRotationConstraint>();
        MultiPositionConstraint headBone_PosConstraint = headBone_Constraint.GetComponent<MultiPositionConstraint>();

        Transform headBoneRotCon = headBone_RotConstraint.data.constrainedObject;
        headBoneRotCon = headBone.transform;
        headBone_RotConstraint.data.constrainedObject = headBoneRotCon;


        Transform headBonePosCon = headBone_PosConstraint.data.constrainedObject;
        headBonePosCon = headBone.transform;
        headBone_PosConstraint.data.constrainedObject = headBonePosCon;

        rigs.Build();
    }

}
