using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    public class WeightedTransformArrayBinder
    {
        public static void BindReadOnlyTransforms(Animator animator, Component component, WeightedTransformArray weightedTransformArray, out NativeArray<ReadOnlyTransformHandle> transforms)
        {
            transforms = new NativeArray<ReadOnlyTransformHandle>(weightedTransformArray.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int index = 0; index < weightedTransformArray.Count; ++index)
            {
                transforms[index] = ReadOnlyTransformHandle.Bind(animator, weightedTransformArray[index].transform);
            }
        }

        public static void BindReadWriteTransforms(Animator animator, Component component, WeightedTransformArray weightedTransformArray, out NativeArray<ReadWriteTransformHandle> transforms)
        {
            transforms = new NativeArray<ReadWriteTransformHandle>(weightedTransformArray.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int index = 0; index < weightedTransformArray.Count; ++index)
            {
                transforms[index] = ReadWriteTransformHandle.Bind(animator, weightedTransformArray[index].transform);
            }
        }

        public static void BindWeights(Animator animator, Component component, WeightedTransformArray weightedTransformArray, string name, out NativeArray<PropertyStreamHandle> weights)
        {
            weights = new NativeArray<PropertyStreamHandle>(weightedTransformArray.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int index = 0; index < weightedTransformArray.Count; ++index)
            {
                weights[index] = animator.BindStreamProperty(component.transform, component.GetType(), name + ".m_Item" + index + ".weight");
            }
        }
    }
}
