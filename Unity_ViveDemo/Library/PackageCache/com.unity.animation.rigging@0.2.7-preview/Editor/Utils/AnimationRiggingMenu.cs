using System;
using System.Reflection;
using UnityEngine;

namespace UnityEditor.Animations.Rigging
{
    public static class AnimationRiggingMenu
    {
        static bool FilterSourceAndDestinationFromSelection(out Transform source, out Transform destination)
        {
            var selected = Selection.instanceIDs;
            if (selected == null || selected.Length != 2)
            {
                source = destination = null;
                return false;
            }

            var srcGameObject = EditorUtility.InstanceIDToObject(selected[1]) as GameObject;
            var dstGameObject = EditorUtility.InstanceIDToObject(selected[0]) as GameObject;
            if (srcGameObject == null || dstGameObject == null)
            {
                source = destination = null;
                return false;
            }

            source = srcGameObject.transform;
            destination = dstGameObject.transform;

            return true;
        }

        [MenuItem("Animation Rigging/Align Transform", false, 0)]
        static void PerformTransformAlign()
        {
            if (FilterSourceAndDestinationFromSelection(out Transform src, out Transform dst))
            {
                Undo.RecordObject(dst, "Align transform " + dst.name + " with " + src.name);
                dst.SetPositionAndRotation(src.position, src.rotation);
            }
        }

        [MenuItem("Animation Rigging/Align Rotation", false, 0)]
        static void PerformRotationAlign()
        {
            if (FilterSourceAndDestinationFromSelection(out Transform src, out Transform dst))
            {
                Undo.RecordObject(dst, "Align rotation of " + dst.name + " with " + src.name);
                dst.rotation = src.rotation;
            }
        }

        [MenuItem("Animation Rigging/Align Position", false, 0)]
        static void PerformPositionAlign()
        {
            if (FilterSourceAndDestinationFromSelection(out Transform src, out Transform dst))
            {
                Undo.RecordObject(dst, "Align position of " + dst.name + " with " + src.name);
                dst.position = src.position;
            }
        }

        [MenuItem("Animation Rigging/Restore Bind Pose", false, 11)]
        static void RestoreBindPose()
        {
            var selection = Selection.gameObjects;
            if (selection.Length == 0)
                return;

            Undo.RegisterFullObjectHierarchyUndo(selection[0], "Restore bind pose");

            Type type = Type.GetType("UnityEditor.AvatarSetupTool, UnityEditor");
            if (type != null)
            {
                MethodInfo info = type.GetMethod("SampleBindPose", BindingFlags.Static | BindingFlags.Public);
                if (info != null)
                    info.Invoke(null, new object[] { selection[0] });
            }
        }

        [MenuItem("Animation Rigging/Rig Setup", false, 12)]
        static void RigSetup()
        {
            var selection = Selection.activeTransform;
            if (selection == null)
                return;

            AnimationRiggingEditorUtils.RigSetup(selection);
        }

        [MenuItem("Animation Rigging/Bone Renderer Setup", false, 13)]
        static void BoneRendererSetup()
        {
            var selection = Selection.activeTransform;
            if (selection == null)
                return;

            AnimationRiggingEditorUtils.BoneRendererSetup(selection);
        }
    }
}
