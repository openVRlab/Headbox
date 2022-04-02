using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

class MultiRotationConstraintTests
{
    const float k_Epsilon = 0.05f;

    struct ConstraintTestData
    {
        public RigTestData rigData;
        public MultiRotationConstraint constraint;

        public Quaternion constrainedObjectRestRotation;
    }

    private ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var multiRotationGO = new GameObject("multiPosition");
        var multiRotation = multiRotationGO.AddComponent<MultiRotationConstraint>();
        multiRotation.Reset();

        multiRotationGO.transform.parent = data.rigData.rigGO.transform;

        multiRotation.data.constrainedObject = data.rigData.hipsGO.transform;
        data.constrainedObjectRestRotation = multiRotation.data.constrainedObject.rotation;

        var sources = new WeightedTransformArray();
        var src0GO = new GameObject("source0");
        var src1GO = new GameObject("source1");
        src0GO.transform.parent = multiRotationGO.transform;
        src1GO.transform.parent = multiRotationGO.transform;
        sources.Add(new WeightedTransform(src0GO.transform, 0f));
        sources.Add(new WeightedTransform(src1GO.transform, 0f));
        multiRotation.data.sourceObjects = sources;

        src0GO.transform.rotation = data.rigData.hipsGO.transform.rotation;
        src1GO.transform.rotation = data.rigData.hipsGO.transform.rotation;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        data.constraint = multiRotation;

        return data;
    }

    [UnityTest]
    public IEnumerator MultiRotationConstraint_FollowSourceObjects()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        // src0.w = 0, src1.w = 0
        Assert.Zero(sources[0].weight);
        Assert.Zero(sources[1].weight);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(constrainedObject.rotation, data.constrainedObjectRestRotation);

        // Add rotation to source objects
        sources[0].transform.rotation *= Quaternion.AngleAxis(90, Vector3.up);
        sources[1].transform.rotation *= Quaternion.AngleAxis(-90, Vector3.up);

        // src0.w = 1, src1.w = 0
        sources[0] = new WeightedTransform(sources[0].transform, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(constrainedObject.rotation, sources[0].transform.rotation);

        // src0.w = 0, src1.w = 1
        sources[0] = new WeightedTransform(sources[0].transform, 0f);
        sources[1] = new WeightedTransform(sources[1].transform, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(constrainedObject.rotation, sources[1].transform.rotation);
    }

    [UnityTest]
    public IEnumerator MultiRotationConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        sources[0].transform.rotation *= Quaternion.AngleAxis(90, Vector3.up);
        sources[0] = new WeightedTransform(sources[0].transform, 1f);
        constraint.data.sourceObjects = sources;

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            Quaternion weightedRot = Quaternion.Lerp(data.constrainedObjectRestRotation, sources[0].transform.rotation, w);
            Assert.AreEqual(
                constrainedObject.rotation,
                weightedRot,
                String.Format("Expected constrainedObject rotation to be {0} for a weight of {1}, but was {2}", weightedRot, w, constrainedObject.rotation)
                );
        }
    }
}
