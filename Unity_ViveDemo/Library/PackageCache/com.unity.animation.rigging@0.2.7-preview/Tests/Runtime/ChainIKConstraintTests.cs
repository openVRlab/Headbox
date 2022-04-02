using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

class ChainIKConstraintTests {

    const float k_Epsilon = 0.05f;

    struct ConstraintTestData
    {
        public RigTestData rigData;
        public ChainIKConstraint constraint;
    }

    private ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var chainIKGO = new GameObject("chainIK");
        var chainIK = chainIKGO.AddComponent<ChainIKConstraint>();
        chainIK.Reset();

        chainIKGO.transform.parent = data.rigData.rigGO.transform;

        chainIK.data.root = data.rigData.hipsGO.transform.Find("Chest");
        Assert.IsNotNull(chainIK.data.root, "Could not find root transform");

        chainIK.data.tip = chainIK.data.root.transform.Find("LeftArm/LeftForeArm/LeftHand");
        Assert.IsNotNull(chainIK.data.tip, "Could not find tip transform");

        var targetGO = new GameObject ("target");
        targetGO.transform.parent = chainIKGO.transform;

        chainIK.data.target = targetGO.transform;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();
        targetGO.transform.position = chainIK.data.tip.position;

        data.constraint = chainIK;

        return data;
    }

    [UnityTest]
    public IEnumerator ChainIKConstraint_FollowsTarget()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var target = constraint.data.target;
        var tip = constraint.data.tip;
        var root = constraint.data.root;

        for (int i = 0; i < 5; ++i)
        {
            target.position += new Vector3(0f, 0.1f, 0f);
            yield return RuntimeRiggingTestFixture.YieldTwoFrames();

            Vector3 rootToTip = (tip.position - root.position).normalized;
            Vector3 rootToTarget = (target.position - root.position).normalized;

            Assert.AreEqual(rootToTarget.x, rootToTip.x, k_Epsilon, String.Format("Expected rootToTip.x to be {0}, but was {1}", rootToTip.x, rootToTarget.x));
            Assert.AreEqual(rootToTarget.y, rootToTip.y, k_Epsilon, String.Format("Expected rootToTip.y to be {0}, but was {1}", rootToTip.y, rootToTarget.y));
            Assert.AreEqual(rootToTarget.z, rootToTip.z, k_Epsilon, String.Format("Expected rootToTip.z to be {0}, but was {1}", rootToTip.z, rootToTarget.z));
        }
    }

    [UnityTest]
    public IEnumerator ChainIKConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        List<Transform> chain = new List<Transform>();
        Transform tmp = constraint.data.tip;
        while (tmp != constraint.data.root)
        {
            chain.Add(tmp);
            tmp = tmp.parent;
        }
        chain.Add(constraint.data.root);
        chain.Reverse();

        // Chain with no constraint.
        Vector3[] bindPoseChain = chain.Select(transform => transform.position).ToArray();

        var target = constraint.data.target;
        target.position += new Vector3(0f, 0.5f, 0f);

        yield return null;

        // Chain with ChainIK constraint.
        Vector3[] weightedChain = chain.Select(transform => transform.position).ToArray();

        // In-between chains.
        List<Vector3[]> inBetweenChains = new List<Vector3[]>();
        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            inBetweenChains.Add(chain.Select(transform => transform.position).ToArray());
        }

        for (int i = 0; i <= 5; ++i)
        {
            Vector3[] prevChain = (i > 0) ? inBetweenChains[i - 1] : bindPoseChain;
            Vector3[] currentChain = inBetweenChains[i];
            Vector3[] nextChain = (i < 5) ? inBetweenChains[i + 1] : weightedChain;

            for (int j = 0; j < bindPoseChain.Length - 1; ++j)
            {
                Vector2 dir1 = prevChain[j + 1] - prevChain[j];
                Vector2 dir2 = currentChain[j + 1] - currentChain[j];
                Vector2 dir3 = nextChain[j + 1] - nextChain[j];

                float maxAngle = Vector2.Angle(dir1, dir3);
                float angle = Vector2.Angle(dir1, dir2);

                Assert.GreaterOrEqual(angle, 0f);
                Assert.LessOrEqual(angle, maxAngle);
            }
        }
    }
}
