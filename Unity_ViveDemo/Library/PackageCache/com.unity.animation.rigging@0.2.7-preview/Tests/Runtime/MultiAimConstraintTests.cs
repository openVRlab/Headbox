using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

class MultiAimConstraintTests
{
    const float k_Epsilon = 1e-5f;

    struct ConstraintTestData
    {
        public RigTestData rigData;
        public MultiAimConstraint constraint;
        public AffineTransform constrainedObjectRestTx;
    }

    private ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var multiAimGO = new GameObject("multiAim");
        var multiAim = multiAimGO.AddComponent<MultiAimConstraint>();
        multiAim.Reset();

        multiAimGO.transform.parent = data.rigData.rigGO.transform;

        var head = data.rigData.hipsGO.transform.Find("Chest/Head");
        multiAim.data.constrainedObject = head;

        var sources = new WeightedTransformArray();
        var src0GO = new GameObject("source0");
        var src1GO = new GameObject("source1");
        src0GO.transform.parent = multiAimGO.transform;
        src1GO.transform.parent = multiAimGO.transform;
        sources.Add(new WeightedTransform(src0GO.transform, 0f));
        sources.Add(new WeightedTransform(src1GO.transform, 0f));
        multiAim.data.sourceObjects = sources;
        multiAim.data.aimAxis = MultiAimConstraintData.Axis.Z;

        data.constrainedObjectRestTx = new AffineTransform(head.position, head.rotation);
        src0GO.transform.SetPositionAndRotation(data.constrainedObjectRestTx.translation + Vector3.forward, data.constrainedObjectRestTx.rotation);
        src1GO.transform.SetPositionAndRotation(data.constrainedObjectRestTx.translation + Vector3.forward, data.constrainedObjectRestTx.rotation);

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();
        data.constraint = multiAim;

        return data;
    }

    [UnityTest]
    public IEnumerator MultiAimConstraint_FollowSourceObjects()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        // Add displacement to source objects
        sources[0].transform.position += Vector3.left;
        sources[1].transform.position += Vector3.right;

        // src0.w = 0, src1.w = 0
        Assert.Zero(sources[0].weight);
        Assert.Zero(sources[1].weight);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(constrainedObject.position, data.constrainedObjectRestTx.translation);
        Assert.AreEqual(constrainedObject.rotation, data.constrainedObjectRestTx.rotation);

        // src0.w = 1, src1.w = 0
        sources.SetWeight(0, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Vector3 currAim = constrainedObject.rotation * Vector3.forward;
        Vector3 src0Dir = (sources[0].transform.position - constrainedObject.position).normalized;
        Vector3 src1Dir = (sources[1].transform.position - constrainedObject.position).normalized;
        Assert.AreEqual(0f, Vector3.Angle(currAim, src0Dir), k_Epsilon);
        Assert.AreNotEqual(0f, Vector3.Angle(currAim, src1Dir));

        // src0.w = 0, src1.w = 1
        sources.SetWeight(0, 0f);
        sources.SetWeight(1, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        currAim = constrainedObject.rotation * Vector3.forward;
        src0Dir = (sources[0].transform.position - constrainedObject.position).normalized;
        src1Dir = (sources[1].transform.position - constrainedObject.position).normalized;
        Assert.AreNotEqual(0f, Vector3.Angle(currAim, src0Dir));
        Assert.AreEqual(0f, Vector3.Angle(currAim, src1Dir), k_Epsilon);
    }

    [UnityTest]
    public IEnumerator MultiAimConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        Assert.Zero(sources[0].weight);
        Assert.Zero(sources[1].weight);

        sources[0].transform.position += Vector3.left;
        sources.SetWeight(0, 1f);
        constraint.data.sourceObjects = sources;

        var src0Pos = sources[0].transform.position;

        var angle = 180f;
        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            var currAim = constrainedObject.rotation * Vector3.forward;
            var src0Dir = (src0Pos - constrainedObject.position).normalized;
            var angleTest = Vector3.Angle(currAim, src0Dir);

            Assert.Less(angleTest, angle, "Angle between currAim and src0Dir should be smaller than last frame since constraint weight is greater.");
            angle = angleTest;
        }
    }
}
