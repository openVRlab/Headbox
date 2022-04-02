using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditor;
using System.Collections;
using NUnit.Framework;
using System.IO;
using System;

public class RuntimeRiggingTestFixture
{
    const string k_PackageName = "com.unity.animation.rigging";

    public struct RigTestData
    {
        public GameObject rootGO;
        public GameObject hipsGO;
        public GameObject rigGO;

        public Animator animator;
    }

    static private string m_PackageRelativePath;
    static public string packageRelativePath
    {
        get
        {
            if (String.IsNullOrEmpty(m_PackageRelativePath))
            {
                m_PackageRelativePath = GetPackageRelativePath();
            }

            return m_PackageRelativePath;
        }
    }

    static public RigTestData SetupRigHierarchy()
    {
        var data = new RigTestData();

        data.rootGO = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(packageRelativePath + "/Tests/Runtime/Assets/Dude_low.fbx")) as GameObject;
        Assert.IsNotNull(data.rootGO, "Could not load rig hierarchy.");

        data.hipsGO = data.rootGO.transform.Find("Reference/Hips").gameObject;
        Assert.IsNotNull(data.hipsGO, "Could not find hips game object in hierarchy.");

        data.rigGO = new GameObject("Rig");
        data.rigGO.transform.parent = data.rootGO.transform;
        var rig = data.rigGO.AddComponent<Rig>();

        data.animator = data.rootGO.GetComponent<Animator>();
        if (data.animator == null)
            data.animator = data.rootGO.AddComponent<Animator>();

        data.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        data.animator.avatar = null;

        var rigBuilder = data.rootGO.AddComponent<RigBuilder>();
        rigBuilder.layers.Add(new RigBuilder.RigLayer(rig));

        return data;
    }

    private static string GetPackageRelativePath()
    {
        string relativePath = "Packages/" + k_PackageName;
        string packagePath = Path.GetFullPath(relativePath);
        if (Directory.Exists(packagePath))
        {
            return relativePath;
        }

        return null;
    }

    public static IEnumerator YieldTwoFrames()
    {
        // this is necessary when we changed the constraint weight in a test, 
        // because test are executed like coroutine so they are called right after all MonoBehaviour.Update thus missing the RigBuilder.Update
        yield return null;
        yield return null;
    }
}
