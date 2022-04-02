using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct MultiAimConstraintJob : IWeightedAnimationJob
    {
        const float k_Epsilon = 1e-5f;

        public ReadWriteTransformHandle driven;
        public ReadOnlyTransformHandle drivenParent;
        public Vector3Property drivenOffset;

        public NativeArray<ReadOnlyTransformHandle> sourceTransforms;
        public NativeArray<PropertyStreamHandle> sourceWeights;
        public NativeArray<Quaternion> sourceOffsets;

        public NativeArray<float> weightBuffer;

        public Vector3 aimAxis;
        public Vector3 axesMask;

        public FloatProperty minLimit;
        public FloatProperty maxLimit;

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

                Vector2 minMaxAngles = new Vector2(minLimit.Get(stream), maxLimit.Get(stream));

                var drivenWPos = driven.GetPosition(stream);
                var drivenLRot = driven.GetLocalRotation(stream);
                var drivenParentInvRot = Quaternion.Inverse(drivenParent.GetRotation(stream));
                Quaternion accumDeltaRot = QuaternionExt.zero;
                var fromDir = drivenLRot * aimAxis;
                float accumWeights = 0f;
                for (int i = 0; i < sourceTransforms.Length; ++i)
                {
                    var normalizedWeight = weightBuffer[i] * weightScale;
                    if (normalizedWeight < k_Epsilon)
                        continue;

                    ReadOnlyTransformHandle sourceTransform = sourceTransforms[i];

                    var toDir = drivenParentInvRot * (sourceTransform.GetPosition(stream) - drivenWPos);
                    if (toDir.sqrMagnitude < k_Epsilon)
                        continue;

                    var crossDir = Vector3.Cross(fromDir, toDir).normalized;
                    if (Vector3.Dot(axesMask, axesMask) < 3f)
                    {
                        crossDir = AnimationRuntimeUtils.Select(Vector3.zero, crossDir, axesMask).normalized;
                        if (Vector3.Dot(crossDir, crossDir) > k_Epsilon)
                        {
                            fromDir = AnimationRuntimeUtils.ProjectOnPlane(fromDir, crossDir);
                            toDir = AnimationRuntimeUtils.ProjectOnPlane(toDir, crossDir);
                        }
                        else
                        {
                            toDir = fromDir;
                        }
                    }

                    var rotToSource = Quaternion.AngleAxis(
                        Mathf.Clamp(Vector3.Angle(fromDir, toDir), minMaxAngles.x, minMaxAngles.y),
                        crossDir
                        );

                    accumDeltaRot = QuaternionExt.Add(
                        accumDeltaRot,
                        QuaternionExt.Scale(sourceOffsets[i] * rotToSource, normalizedWeight)
                        );

                    // Required to update handles with binding info.
                    sourceTransforms[i] = sourceTransform;
                    accumWeights += normalizedWeight;
                }

                accumDeltaRot = QuaternionExt.NormalizeSafe(accumDeltaRot);
                if (accumWeights < 1f)
                    accumDeltaRot = Quaternion.Lerp(Quaternion.identity, accumDeltaRot, accumWeights);

                Quaternion newRot = accumDeltaRot * drivenLRot;
                if (Vector3.Dot(axesMask, axesMask) < 3f)
                    newRot = Quaternion.Euler(AnimationRuntimeUtils.Select(drivenLRot.eulerAngles, newRot.eulerAngles, axesMask));

                var offset = drivenOffset.Get(stream);
                if (Vector3.Dot(offset, offset) > 0f)
                    newRot *= Quaternion.Euler(offset);

                driven.SetLocalRotation(stream, Quaternion.Lerp(drivenLRot, newRot, w));
            }
            else
                AnimationRuntimeUtils.PassThrough(stream, driven);
        }
    }

    public interface IMultiAimConstraintData
    {
        Transform constrainedObject { get; }
        WeightedTransformArray sourceObjects { get; }
        bool maintainOffset { get; }
        Vector3 aimAxis { get; }

        bool constrainedXAxis { get; }
        bool constrainedYAxis { get; }
        bool constrainedZAxis { get; }

        string offsetVector3Property { get; }
        string minLimitFloatProperty { get; }
        string maxLimitFloatProperty { get; }
        string sourceObjectsProperty { get; }
    }

    public class MultiAimConstraintJobBinder<T> : AnimationJobBinder<MultiAimConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiAimConstraintData
    {
        public override MultiAimConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiAimConstraintJob();

            job.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
            job.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
            job.aimAxis = data.aimAxis;

            WeightedTransformArray sourceObjects = data.sourceObjects;

            WeightedTransformArrayBinder.BindReadOnlyTransforms(animator, component, sourceObjects, out job.sourceTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out job.sourceWeights);

            job.sourceOffsets = new NativeArray<Quaternion>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            job.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < sourceObjects.Count; ++i)
            {
                if (data.maintainOffset)
                {
                    var constrainedAim = data.constrainedObject.rotation * data.aimAxis;
                    job.sourceOffsets[i] = QuaternionExt.FromToRotation(
                        sourceObjects[i].transform.position - data.constrainedObject.position,
                        constrainedAim
                        );
                }
                else
                    job.sourceOffsets[i] = Quaternion.identity;
            }

            job.minLimit = FloatProperty.Bind(animator, component, data.minLimitFloatProperty);
            job.maxLimit = FloatProperty.Bind(animator, component, data.maxLimitFloatProperty);
            job.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);

            job.axesMask = new Vector3(
                System.Convert.ToSingle(data.constrainedXAxis),
                System.Convert.ToSingle(data.constrainedYAxis),
                System.Convert.ToSingle(data.constrainedZAxis)
                );

            return job;
        }

        public override void Destroy(MultiAimConstraintJob job)
        {
            job.sourceTransforms.Dispose();
            job.sourceWeights.Dispose();
            job.sourceOffsets.Dispose();
            job.weightBuffer.Dispose();
        }
    }
}
