using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

class MultiPositionConstraintTests
{
    const float k_Epsilon = 0.05f;

    struct ConstraintTestData
    {
        public RigTestData rigData;
        public MultiPositionConstraint constraint;

        public Vector3 constrainedObjectRestPosition;
    }

    private ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var multiPositionGO = new GameObject("multiPosition");
        var multiPosition = multiPositionGO.AddComponent<MultiPositionConstraint>();
        multiPosition.Reset();

        multiPositionGO.transform.parent = data.rigData.rigGO.transform;

        multiPosition.data.constrainedObject = data.rigData.hipsGO.transform;
        data.constrainedObjectRestPosition = multiPosition.data.constrainedObject.position;

        var sources = new WeightedTransformArray();
        var src0GO = new GameObject("source0");
        var src1GO = new GameObject("source1");
        src0GO.transform.parent = multiPositionGO.transform;
        src1GO.transform.parent = multiPositionGO.transform;
        sources.Add(new WeightedTransform(src0GO.transform, 0f));
        sources.Add(new WeightedTransform(src1GO.transform, 0f));
        multiPosition.data.sourceObjects = sources;

        src0GO.transform.position = data.rigData.hipsGO.transform.position;
        src1GO.transform.position = data.rigData.hipsGO.transform.position;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        data.constraint = multiPosition;

        return data;
    }

    [UnityTest]
    public IEnumerator MultiPositionConstraint_FollowSourceObjects()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        // src0.w = 0, src1.w = 0
        Assert.Zero(sources[0].weight);
        Assert.Zero(sources[1].weight);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(constrainedObject.position, data.constrainedObjectRestPosition);

        // Add displacement to source objects
        sources[0].transform.position += Vector3.right;
        sources[1].transform.position += Vector3.left;

        // src0.w = 1, src1.w = 0
        sources.SetWeight(0, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(constrainedObject.position, sources[0].transform.position);

        // src0.w = 0, src1.w = 1
        sources.SetWeight(0, 0f);
        sources.SetWeight(1, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(constrainedObject.position, sources[1].transform.position);

        // src0.w = 1, src1.w = 1
        // since source object positions are mirrored, we should simply evaluate to the original rest pos.
        sources.SetWeight(0, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(constrainedObject.position, data.constrainedObjectRestPosition);
    }

    [UnityTest]
    public IEnumerator MultiPositionConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        sources[0].transform.position += Vector3.forward;
        sources.SetWeight(0, 1f);
        constraint.data.sourceObjects = sources;

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            Vector3 weightedPos = Vector3.Lerp(data.constrainedObjectRestPosition, sources[0].transform.position, w);
            Assert.AreEqual(
                constrainedObject.position,
                weightedPos,
                String.Format("Expected constrainedObject to be at {0} for a weight of {1}, but was {2}", weightedPos, w, constrainedObject.position)
                );
        }
    }
}
