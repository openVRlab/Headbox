using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

class MultiReferentialConstraintTests
{
    const float k_Epsilon = 0.000001f;

    struct ConstraintTestData
    {
        public RigTestData rigData;
        public MultiReferentialConstraint constraint;

        public AffineTransform restPose;
    }

    private ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var multiRefGO = new GameObject("multiReferential");
        var multiRef = multiRefGO.AddComponent<MultiReferentialConstraint>();
        multiRef.Reset();
        
        multiRefGO.transform.parent = data.rigData.rigGO.transform;

        List<Transform> sources = new List<Transform>(3);
        var src0GO = new GameObject("source0");
        var src1GO = new GameObject("source1");
        src0GO.transform.parent = multiRefGO.transform;
        src1GO.transform.parent = multiRefGO.transform;
        sources.Add(data.rigData.hipsGO.transform);
        sources.Add(src0GO.transform);
        sources.Add(src1GO.transform);
        multiRef.data.sourceObjects = sources;
        multiRef.data.driver = 0;

        var pos = data.rigData.hipsGO.transform.position;
        var rot = data.rigData.hipsGO.transform.rotation;
        src0GO.transform.SetPositionAndRotation(pos, rot);
        src1GO.transform.SetPositionAndRotation(pos, rot);
        data.restPose = new AffineTransform(pos, rot);

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        data.constraint = multiRef;

        return data;
    }

    [UnityTest]
    public IEnumerator MultiReferentialConstraint_FollowSourceObjects()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;
        
        var sources = constraint.data.sourceObjects;

        constraint.data.driver = 0;
        var driver = sources[0];
        driver.position += Vector3.forward;
        driver.rotation *= Quaternion.AngleAxis(90, Vector3.up);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(driver.position, sources[1].position);
        Assert.AreEqual(driver.rotation, sources[1].rotation);
        Assert.AreEqual(driver.position, sources[2].position);
        Assert.AreEqual(driver.rotation, sources[2].rotation);

        constraint.data.driver = 1;
        driver = sources[1];
        driver.position += Vector3.back;
        driver.rotation *= Quaternion.AngleAxis(-90, Vector3.up);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(driver.position, sources[0].position);
        Assert.AreEqual(driver.rotation, sources[0].rotation);
        Assert.AreEqual(driver.position, sources[2].position);
        Assert.AreEqual(driver.rotation, sources[2].rotation);

        constraint.data.driver = 2;
        driver = sources[2];
        driver.position += Vector3.up;
        driver.rotation *= Quaternion.AngleAxis(90, Vector3.left);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.AreEqual(driver.position, sources[0].position);
        Assert.AreEqual(driver.rotation, sources[0].rotation);
        Assert.AreEqual(driver.position, sources[1].position);
        Assert.AreEqual(driver.rotation, sources[1].rotation);
    }

    [UnityTest]
    public IEnumerator MultiReferentialConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var sources = constraint.data.sourceObjects;

        constraint.data.driver = 1;
        constraint.weight = 0f;
        yield return null;

        sources[1].position += Vector3.right;
        sources[1].rotation *= Quaternion.AngleAxis(-90, Vector3.up);

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            constraint.weight = w;
            yield return null;

            var weightedPos = Vector3.Lerp(data.restPose.translation, sources[1].position, w);
            Assert.AreEqual(
                sources[0].position,
                weightedPos,
                String.Format("Expected Source0 to be at {0} for a weight of {1}, but was {2}", weightedPos, w, sources[0].position)
                );
            Assert.AreEqual(
                sources[2].position,
                weightedPos,
                String.Format("Expected Source2 to be at {0} for a weight of {1}, but was {2}", weightedPos, w, sources[2].position)
                );

            var weightedRot = Quaternion.Lerp(data.restPose.rotation, sources[1].rotation, w);
            RotationsAreEqual(sources[0].rotation, weightedRot, w);
            RotationsAreEqual(sources[2].rotation, weightedRot, w);

            // Since we have no animation in the stream the new rest pose
            // should be the last evaluated one.
            data.restPose.translation = sources[0].position;
            data.restPose.rotation = sources[0].rotation;
        }
    }

    static void RotationsAreEqual(Quaternion lhs, Quaternion rhs, float w)
    {
        var dot = Quaternion.Dot(lhs, rhs);
        Assert.AreEqual(
            Mathf.Abs(dot),
            1f,
            k_Epsilon,
            String.Format("Expected rotations to be equal for a weight of {0}", w)
            );
    }
}
