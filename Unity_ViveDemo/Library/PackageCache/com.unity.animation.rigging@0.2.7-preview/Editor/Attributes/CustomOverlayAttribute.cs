using UnityEngine;

namespace UnityEditor.Animations.Rigging
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CustomOverlayAttribute : Attribute
    {
        private Type m_EffectorType;

        public CustomOverlayAttribute(Type effectorType)
        {
            m_EffectorType = effectorType;
        }

        public Type effectorType { get => m_EffectorType; }
    }
}
