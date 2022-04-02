using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using System;
using System.Reflection;
using System.Collections;

namespace UnityEditor.Animations.Rigging
{
    [InitializeOnLoad]
    public static class SceneViewOverlay
    {
        static Type m_SceneViewOverlayType = null;
        static Type m_WindowDisplayOptionType = null;
        static Type m_WindowFunctionDelegateType = null;

        static MethodInfo m_WindowFunc = null;

        public delegate void WindowFunction(UnityEngine.Object target, SceneView sceneView);

        static SceneViewOverlay()
        {
            m_SceneViewOverlayType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneViewOverlay");
            m_WindowDisplayOptionType = m_SceneViewOverlayType.GetNestedType("WindowDisplayOption");
            m_WindowFunctionDelegateType = m_SceneViewOverlayType.GetNestedType("WindowFunction");

			m_WindowFunc = m_SceneViewOverlayType.GetMethod(
                    "Window",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    CallingConventions.Any,
                    new Type[] { typeof(GUIContent), m_WindowFunctionDelegateType, typeof(int), m_WindowDisplayOptionType},
                    null);
        }

        public static void Window(GUIContent title, WindowFunction sceneViewFunc, int order)
        {
            var windowFunctionDelegate = Delegate.CreateDelegate(m_WindowFunctionDelegateType, null, sceneViewFunc.Method);

            if (m_WindowFunc != null)
                m_WindowFunc.Invoke(null, BindingFlags.InvokeMethod, null, new System.Object[] {title, windowFunctionDelegate, order, Enum.ToObject(m_WindowDisplayOptionType , 2)}, null);
        }

    }


}
