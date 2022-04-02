namespace UnityEngine.Animations.Rigging
{
    using System;

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SyncSceneToStreamAttribute : Attribute { }

    public enum PropertyType : byte { Bool, Int, Float };

    public struct PropertyDescriptor
    {
        public int size;
        public PropertyType type;
    }

    public struct Property
    {
        public string name;
        public PropertyDescriptor descriptor;
    }

    public struct RigProperties
    {
        public static string s_Weight = "m_Weight";
        public Component component;
    }

    public struct ConstraintProperties
    {
        public static string s_Weight = "m_Weight";
        public Component component;
        public Property[] properties;
    }

    public static class PropertyUtils
    {
        public static string ConstructConstraintDataPropertyName(string property)
        {
            return "m_Data." + property;
        }

        public static string ConstructCustomPropertyName(Component component, string property)
        {
            return component.transform.GetInstanceID() + "/" + component.GetType() + "/" + property;
        }
    }
}