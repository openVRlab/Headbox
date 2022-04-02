using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct MultiRotationConstraintJob : IWeightedAnimationJob
    {
        const float k_Epsilon = 1e-5f;

        public ReadWriteTransformHandle driven;
        public ReadOnlyTransformHandle drivenParent;
        public Vector3Property drivenOffset;

        public NativeArray<ReadOnlyTransformHandle> sourceTransforms;
        public NativeArray<PropertyStreamHandle> sourceWeights;
        public NativeArray<Quaternion> sourceOffsets;

        public NativeArray<float> weightBuffer;

        public Vector3 axesMask;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                AnimationStreamHandleUtility.ReadFloats(stream, sourceWeights, weightBuffer);

                float sumWeights = AnimationRuntimeUtils.Sum(weightBuffer);
                if (sumWeights < k_Epsilon)
                {
                    AnimationRuntimeUtils.PassThrough(stream, driven);
                    return;
                }

                float weightScale = sumWeights > 1f ? 1f / sumWeights : 1f;

                float accumWeights = 0f;
                Quaternion accumRot = QuaternionExt.zero;
                for (int i = 0; i < sourceTransforms.Length; ++i)
                {
                    var normalizedWeight = weightBuffer[i] * weightScale;
                    if (normalizedWeight < k_Epsilon)
                        continue;

                    ReadOnlyTransformHandle sourceTransform = sourceTransforms[i];
                    accumRot = QuaternionExt.Add(accumRot, QuaternionExt.Scale(sourceTransform.GetRotation(stream) * sourceOffsets[i], normalizedWeight));

                    // Required to update handles with binding info.
                    sourceTransforms[i] = sourceTransform;
                    accumWeights += normalizedWeight;
                }

                accumRot = QuaternionExt.NormalizeSafe(accumRot);
                if (accumWeights < 1f)
                    accumRot = Quaternion.Lerp(driven.GetRotation(stream), accumRot, accumWeights);

                // Convert accumRot to local space
                if (drivenParent.IsValid(stream))
                    accumRot = Quaternion.Inverse(drivenParent.GetRotation(stream)) * accumRot;

                Quaternion currentLRot = driven.GetLocalRotation(stream);
                if (Vector3.Dot(axesMask, axesMask) < 3f)
                    accumRot = Quaternion.Euler(AnimationRuntimeUtils.Lerp(currentLRot.eulerAngles, accumRot.eulerAngles, axesMask));

                var offset = drivenOffset.Get(stream);
                if (Vector3.Dot(offset, offset) > 0f)
                    accumRot *= Quaternion.Euler(offset);

                driven.SetLocalRotation(stream, Quaternion.Lerp(currentLRot, accumRot, w));
            }
            else
                AnimationRuntimeUtils.PassThrough(stream, driven);
        }
    }

    public interface IMultiRotationConstraintData
    {
        Transform constrainedObject { get; }
        WeightedTransformArray sourceObjects { get; }
        bool maintainOffset { get; }

        string offsetVector3Property { get; }
        string sourceObjectsProperty { get; }

        bool constrainedXAxis { get; }
        bool constrainedYAxis { get; }
        bool constrainedZAxis { get; }
    }

    public class MultiRotationConstraintJobBinder<T> : AnimationJobBinder<MultiRotationConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiRotationConstraintData
    {
        public override MultiRotationConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiRotationConstraintJob();

            job.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
            job.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
            job.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);

            WeightedTransformArray sourceObjects = data.sourceObjects;

            WeightedTransformArrayBinder.BindReadOnlyTransforms(animator, component, sourceObjects, out job.sourceTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out job.sourceWeights);

            job.sourceOffsets = new NativeArray<Quaternion>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            job.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            Quaternion drivenRot = data.constrainedObject.rotation;
            for (int i = 0; i < sourceObjects.Count; ++i)
            {
                job.sourceOffsets[i] = data.maintainOffset ?
                    (Quaternion.Inverse(sourceObjects[i].transform.rotation) * drivenRot) : Quaternion.identity;
            }

            job.axesMask = new Vector3(
                System.Convert.ToSingle(data.constrainedXAxis),
                System.Convert.ToSingle(data.constrainedYAxis),
                System.Convert.ToSingle(data.constrainedZAxis)
                );

            return job;
        }

        public override void Destroy(MultiRotationConstraintJob job)
        {
            job.sourceTransforms.Dispose();
            job.sourceWeights.Dispose();
            job.sourceOffsets.Dispose();
            job.weightBuffer.Dispose();
        }
    }
}
