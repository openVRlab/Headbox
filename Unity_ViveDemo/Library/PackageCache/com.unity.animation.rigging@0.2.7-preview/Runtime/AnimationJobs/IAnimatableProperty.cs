namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    public interface IAnimatableProperty<T>
    {
        T Get(AnimationStream stream);
        void Set(AnimationStream stream, T value);
    }

    public struct BoolProperty : IAnimatableProperty<bool>
    {
        public PropertyStreamHandle value;

        public static BoolProperty Bind(Animator animator, Component component, string name)
        {
            return new BoolProperty()
            {
                value = animator.BindStreamProperty(component.transform, component.GetType(), name)
            };
        }

        public static BoolProperty BindCustom(Animator animator, string property)
        {
            return new BoolProperty
            {
                value = animator.BindCustomStreamProperty(property, CustomStreamPropertyType.Bool)
            };
        }
        
        public bool Get(AnimationStream stream) => value.GetBool(stream);
        public void Set(AnimationStream stream, bool v) => value.SetBool(stream, v);
    }

    public struct IntProperty : IAnimatableProperty<int>
    {
        public PropertyStreamHandle value;

        public static IntProperty Bind(Animator animator, Component component, string name)
        {
            return new IntProperty()
            {
                value = animator.BindStreamProperty(component.transform, component.GetType(), name)
            };
        }

        public static IntProperty BindCustom(Animator animator, string property)
        {
            return new IntProperty
            {
                value = animator.BindCustomStreamProperty(property, CustomStreamPropertyType.Int)
            };
        }

        public int Get(AnimationStream stream) => value.GetInt(stream);
        public void Set(AnimationStream stream, int v) => value.SetInt(stream, v);
    }

    public struct FloatProperty : IAnimatableProperty<float>
    {
        public PropertyStreamHandle value;

        public static FloatProperty Bind(Animator animator, Component component, string name)
        {
            return new FloatProperty()
            {
                value = animator.BindStreamProperty(component.transform, component.GetType(), name)
            };
        }

        public static FloatProperty BindCustom(Animator animator, string property)
        {
            return new FloatProperty
            {
                value = animator.BindCustomStreamProperty(property, CustomStreamPropertyType.Float)
            };
        }

        public float Get(AnimationStream stream) => value.GetFloat(stream);
        public void Set(AnimationStream stream, float v) => value.SetFloat(stream, v);
    }

    public struct Vector2Property : IAnimatableProperty<Vector2>
    {
        public PropertyStreamHandle x;
        public PropertyStreamHandle y;

        public static Vector2Property Bind(Animator animator, Component component, string name)
        {
            var type = component.GetType();
            return new Vector2Property
            {
                x = animator.BindStreamProperty(component.transform, type, name + ".x"),
                y = animator.BindStreamProperty(component.transform, type, name + ".y")
            };
        }

        public static Vector2Property BindCustom(Animator animator, string name)
        {
            return new Vector2Property
            {
                x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Float),
                y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Float)
            };
        }

        public Vector2 Get(AnimationStream stream) =>
            new Vector2(x.GetFloat(stream), y.GetFloat(stream));

        public void Set(AnimationStream stream, Vector2 value)
        {
            x.SetFloat(stream, value.x);
            y.SetFloat(stream, value.y);
        }
    }

    public struct Vector3Property : IAnimatableProperty<Vector3>
    {
        public PropertyStreamHandle x;
        public PropertyStreamHandle y;
        public PropertyStreamHandle z;

        public static Vector3Property Bind(Animator animator, Component component, string name)
        {
            var type = component.GetType();
            return new Vector3Property
            {
                x = animator.BindStreamProperty(component.transform, type, name + ".x"),
                y = animator.BindStreamProperty(component.transform, type, name + ".y"),
                z = animator.BindStreamProperty(component.transform, type, name + ".z")
            };
        }

        public static Vector3Property BindCustom(Animator animator, string name)
        {
            return new Vector3Property
            {
                x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Float),
                y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Float),
                z = animator.BindCustomStreamProperty(name + ".z", CustomStreamPropertyType.Float)
            };
        }

        public Vector3 Get(AnimationStream stream) =>
            new Vector3(x.GetFloat(stream), y.GetFloat(stream), z.GetFloat(stream));

        public void Set(AnimationStream stream, Vector3 value)
        {
            x.SetFloat(stream, value.x);
            y.SetFloat(stream, value.y);
            z.SetFloat(stream, value.z);
        }
    }

    public struct Vector3IntProperty : IAnimatableProperty<Vector3Int>
    {
        public PropertyStreamHandle x;
        public PropertyStreamHandle y;
        public PropertyStreamHandle z;

        public static Vector3IntProperty Bind(Animator animator, Component component, string name)
        {
            var type = component.GetType();
            return new Vector3IntProperty
            {
                x = animator.BindStreamProperty(component.transform, type, name + ".x"),
                y = animator.BindStreamProperty(component.transform, type, name + ".y"),
                z = animator.BindStreamProperty(component.transform, type, name + ".z")
            };
        }

        public static Vector3IntProperty BindCustom(Animator animator, string name)
        {
            return new Vector3IntProperty
            {
                x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Int),
                y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Int),
                z = animator.BindCustomStreamProperty(name + ".z", CustomStreamPropertyType.Int)
            };
        }

        public Vector3Int Get(AnimationStream stream) =>
            new Vector3Int(x.GetInt(stream), y.GetInt(stream), z.GetInt(stream));

        public void Set(AnimationStream stream, Vector3Int value)
        {
            x.SetInt(stream, value.x);
            y.SetInt(stream, value.y);
            z.SetInt(stream, value.z);
        }
    }

    public struct Vector3BoolProperty : IAnimatableProperty<Vector3Bool>
    {
        public PropertyStreamHandle x;
        public PropertyStreamHandle y;
        public PropertyStreamHandle z;

        public static Vector3BoolProperty Bind(Animator animator, Component component, string name)
        {
            var type = component.GetType();
            return new Vector3BoolProperty
            {
                x = animator.BindStreamProperty(component.transform, type, name + ".x"),
                y = animator.BindStreamProperty(component.transform, type, name + ".y"),
                z = animator.BindStreamProperty(component.transform, type, name + ".z")
            };
        }

        public static Vector3BoolProperty BindCustom(Animator animator, string name)
        {
            return new Vector3BoolProperty
            {
                x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Bool),
                y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Bool),
                z = animator.BindCustomStreamProperty(name + ".z", CustomStreamPropertyType.Bool)
            };
        }

        public Vector3Bool Get(AnimationStream stream) =>
            new Vector3Bool(x.GetBool(stream), y.GetBool(stream), z.GetBool(stream));

        public void Set(AnimationStream stream, Vector3Bool value)
        {
            x.SetBool(stream, value.x);
            y.SetBool(stream, value.y);
            z.SetBool(stream, value.z);
        }
    }

    public struct Vector4Property : IAnimatableProperty<Vector4>
    {
        public PropertyStreamHandle x;
        public PropertyStreamHandle y;
        public PropertyStreamHandle z;
        public PropertyStreamHandle w;

        public static Vector4Property Bind(Animator animator, Component component, string name)
        {
            var type = component.GetType();
            return new Vector4Property
            {
                x = animator.BindStreamProperty(component.transform, type, name + ".x"),
                y = animator.BindStreamProperty(component.transform, type, name + ".y"),
                z = animator.BindStreamProperty(component.transform, type, name + ".z"),
                w = animator.BindStreamProperty(component.transform, type, name + ".w")
            };
        }

        public static Vector4Property BindCustom(Animator animator, string name)
        {
            return new Vector4Property
            {
                x = animator.BindCustomStreamProperty(name + ".x", CustomStreamPropertyType.Float),
                y = animator.BindCustomStreamProperty(name + ".y", CustomStreamPropertyType.Float),
                z = animator.BindCustomStreamProperty(name + ".z", CustomStreamPropertyType.Float),
                w = animator.BindCustomStreamProperty(name + ".w", CustomStreamPropertyType.Float)
            };
        }

        public Vector4 Get(AnimationStream stream) =>
            new Vector4(x.GetFloat(stream), y.GetFloat(stream), z.GetFloat(stream), w.GetFloat(stream));

        public void Set(AnimationStream stream, Vector4 value)
        {
            x.SetFloat(stream, value.x);
            y.SetFloat(stream, value.y);
            z.SetFloat(stream, value.z);
            w.SetFloat(stream, value.w);
        }
    }
}
