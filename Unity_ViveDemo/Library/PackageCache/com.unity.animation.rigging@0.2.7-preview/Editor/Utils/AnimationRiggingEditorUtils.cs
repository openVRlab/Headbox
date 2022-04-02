using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    internal static class AnimationRiggingEditorUtils
    {
        public static void RigSetup(Transform transform)
        {
            var rigBuilder = transform.GetComponent<RigBuilder>();

            if (rigBuilder == null)
                rigBuilder = Undo.AddComponent<RigBuilder>(transform.gameObject);
            else
                Undo.RecordObject(rigBuilder, "Rig Builder Component Added.");

            var name = "Rig";
            var cnt = 1;
            while (rigBuilder.transform.Find(string.Format("{0} {1}", name, cnt)) != null)
            {
                cnt++;
            }
            name = string.Format("{0} {1}", name, cnt);
            var rigGameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(rigGameObject, name);
            rigGameObject.transform.SetParent(rigBuilder.transform);

            var rig = Undo.AddComponent<Rig>(rigGameObject);
            rigBuilder.layers.Add(new RigBuilder.RigLayer(rig));

            if (PrefabUtility.IsPartOfPrefabInstance(rigBuilder))
                EditorUtility.SetDirty(rigBuilder);
        }

        public static void BoneRendererSetup(Transform transform)
        {
            var boneRenderer = transform.GetComponent<BoneRenderer>();
            if (boneRenderer == null)
                boneRenderer = Undo.AddComponent<BoneRenderer>(transform.gameObject);
            else
                Undo.RecordObject(boneRenderer, "Bone renderer setup.");

            var animator = transform.GetComponent<Animator>();
            var renderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            var bones = new List<Transform>();
            if (animator != null && renderers != null && renderers.Length > 0)
            {
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var renderer = renderers[i];
                    for (int j = 0; j < renderer.bones.Length; ++j)
                    {
                        var bone = renderer.bones[j];
                        if (!bones.Contains(bone))
                        {
                            bones.Add(bone);

                            for (int k = 0; k < bone.childCount; k++)
                            {
                                if (!bones.Contains(bone.GetChild(k)))
                                    bones.Add(bone.GetChild(k));
                            }
                        }
                    }
                }
            }
            else
            {
                bones.AddRange(transform.GetComponentsInChildren<Transform>());
            }

            boneRenderer.transforms = bones.ToArray();

            if (PrefabUtility.IsPartOfPrefabInstance(boneRenderer))
                EditorUtility.SetDirty(boneRenderer);
        }
    }
}
