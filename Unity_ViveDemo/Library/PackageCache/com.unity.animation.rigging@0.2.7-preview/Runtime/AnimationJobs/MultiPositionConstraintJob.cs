using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct MultiPositionConstraintJob : IWeightedAnimationJob
    {
        const float k_Epsilon = 1e-5f;

        public ReadWriteTransformHandle driven;
        public ReadOnlyTransformHandle drivenParent;
        public Vector3Property drivenOffset;

        public NativeArray<ReadOnlyTransformHandle> sourceTransforms;
        public NativeArray<PropertyStreamHandle> sourceWeights;
        public NativeArray<Vector3> sourceOffsets;

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

                Vector3 currentWPos = driven.GetPosition(stream);
                Vector3 accumPos = currentWPos;
                for (int i = 0; i < sourceTransforms.Length; ++i)
                {
                    var normalizedWeight = weightBuffer[i] * weightScale;
                    if (normalizedWeight < k_Epsilon)
                        continue;

                    ReadOnlyTransformHandle sourceTransform = sourceTransforms[i];
                    accumPos += (sourceTransform.GetPosition(stream) + sourceOffsets[i] - currentWPos) * normalizedWeight;

                    // Required to update handles with binding info.
                    sourceTransforms[i] = sourceTransform;
                }

                // Convert accumPos to local space
                if (drivenParent.IsValid(stream))
                {
                    drivenParent.GetGlobalTR(stream, out Vector3 parentWPos, out Quaternion parentWRot);
                    var parentTx = new AffineTransform(parentWPos, parentWRot);
                    accumPos = parentTx.InverseTransform(accumPos);
                }

                Vector3 currentLPos = driven.GetLocalPosition(stream);
                if (Vector3.Dot(axesMask, axesMask) < 3f)
                    accumPos = AnimationRuntimeUtils.Lerp(currentLPos, accumPos, axesMask);

                driven.SetLocalPosition(stream, Vector3.Lerp(currentLPos, accumPos + drivenOffset.Get(stream), w));
            }
            else
                AnimationRuntimeUtils.PassThrough(stream, driven);
        }
    }

    public interface IMultiPositionConstraintData
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

    public class MultiPositionConstraintJobBinder<T> : AnimationJobBinder<MultiPositionConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiPositionConstraintData
    {
        public override MultiPositionConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiPositionConstraintJob();

            job.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
            job.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
            job.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);

            WeightedTransformArray sourceObjects = data.sourceObjects;

            WeightedTransformArrayBinder.BindReadOnlyTransforms(animator, component, sourceObjects, out job.sourceTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out job.sourceWeights);

            job.sourceOffsets = new NativeArray<Vector3>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            job.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            Vector3 drivenPos = data.constrainedObject.position;
            for (int i = 0; i < sourceObjects.Count; ++i)
            {
                job.sourceOffsets[i] = data.maintainOffset ? (drivenPos - sourceObjects[i].transform.position) : Vector3.zero;
            }

            job.axesMask = new Vector3(
                System.Convert.ToSingle(data.constrainedXAxis),
                System.Convert.ToSingle(data.constrainedYAxis),
                System.Convert.ToSingle(data.constrainedZAxis)
                );

            return job;
        }

        public override void Destroy(MultiPositionConstraintJob job)
        {
            job.sourceTransforms.Dispose();
            job.sourceWeights.Dispose();
            job.sourceOffsets.Dispose();
            job.weightBuffer.Dispose();
        }
    }
}
