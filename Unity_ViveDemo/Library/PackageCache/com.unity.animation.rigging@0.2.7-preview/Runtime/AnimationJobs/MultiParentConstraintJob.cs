using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct MultiParentConstraintJob : IWeightedAnimationJob
    {
        const float k_Epsilon = 1e-5f;

        public ReadWriteTransformHandle driven;
        public ReadOnlyTransformHandle drivenParent;

        public NativeArray<ReadOnlyTransformHandle> sourceTransforms;
        public NativeArray<PropertyStreamHandle> sourceWeights;
        public NativeArray<AffineTransform> sourceOffsets;

        public NativeArray<float> weightBuffer;

        public Vector3 positionAxesMask;
        public Vector3 rotationAxesMask;

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
                var accumTx = new AffineTransform(Vector3.zero, QuaternionExt.zero);
                for (int i = 0; i < sourceTransforms.Length; ++i)
                {
                    ReadOnlyTransformHandle sourceTransform = sourceTransforms[i];
                    var normalizedWeight = weightBuffer[i] * weightScale;
                    if (normalizedWeight < k_Epsilon)
                        continue;

                    sourceTransform.GetGlobalTR(stream, out Vector3 srcWPos, out Quaternion srcWRot);
                    var sourceTx = new AffineTransform(srcWPos, srcWRot);
                    sourceTx *= sourceOffsets[i];

                    accumTx.translation += sourceTx.translation * normalizedWeight;
                    accumTx.rotation = QuaternionExt.Add(accumTx.rotation, QuaternionExt.Scale(sourceTx.rotation, normalizedWeight));

                    // Required to update handles with binding info.
                    sourceTransforms[i] = sourceTransform;
                    accumWeights += normalizedWeight;
                }

                accumTx.rotation = QuaternionExt.NormalizeSafe(accumTx.rotation);
                if (accumWeights < 1f)
                {
                    driven.GetGlobalTR(stream, out Vector3 currentWPos, out Quaternion currentWRot);
                    accumTx.translation += currentWPos * (1f - accumWeights);
                    accumTx.rotation = Quaternion.Lerp(currentWRot, accumTx.rotation, accumWeights);
                }

                // Convert accumTx to local space
                if (drivenParent.IsValid(stream))
                {
                    drivenParent.GetGlobalTR(stream, out Vector3 parentWPos, out Quaternion parentWRot);
                    var parentTx = new AffineTransform(parentWPos, parentWRot);
                    accumTx = parentTx.InverseMul(accumTx);
                }

                driven.GetLocalTRS(stream, out Vector3 currentLPos, out Quaternion currentLRot, out Vector3 currentLScale);
                if (Vector3.Dot(positionAxesMask, positionAxesMask) < 3f)
                    accumTx.translation = AnimationRuntimeUtils.Lerp(currentLPos, accumTx.translation, positionAxesMask);
                if (Vector3.Dot(rotationAxesMask, rotationAxesMask) < 3f)
                    accumTx.rotation = Quaternion.Euler(AnimationRuntimeUtils.Lerp(currentLRot.eulerAngles, accumTx.rotation.eulerAngles, rotationAxesMask));

                driven.SetLocalTRS(
                    stream,
                    Vector3.Lerp(currentLPos, accumTx.translation, w),
                    Quaternion.Lerp(currentLRot, accumTx.rotation, w),
                    currentLScale
                    );
            }
            else
                AnimationRuntimeUtils.PassThrough(stream, driven);
        }
    }

    public interface IMultiParentConstraintData
    {
        Transform constrainedObject { get; }
        WeightedTransformArray sourceObjects { get; }
        bool maintainPositionOffset { get; }
        bool maintainRotationOffset { get; }

        bool constrainedPositionXAxis { get; }
        bool constrainedPositionYAxis { get; }
        bool constrainedPositionZAxis { get; }
        bool constrainedRotationXAxis { get; }
        bool constrainedRotationYAxis { get; }
        bool constrainedRotationZAxis { get; }

        string sourceObjectsProperty { get; }
    }

    public class MultiParentConstraintJobBinder<T> : AnimationJobBinder<MultiParentConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiParentConstraintData
    {
        public override MultiParentConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiParentConstraintJob();

            job.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
            job.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);

            WeightedTransformArray sourceObjects = data.sourceObjects;

            WeightedTransformArrayBinder.BindReadOnlyTransforms(animator, component, sourceObjects, out job.sourceTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out job.sourceWeights);

            job.sourceOffsets = new NativeArray<AffineTransform>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            job.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            var drivenTx = new AffineTransform(data.constrainedObject.position, data.constrainedObject.rotation);
            for (int i = 0; i < sourceObjects.Count; ++i)
            {
                var sourceTransform = sourceObjects[i].transform;

                var srcTx = new AffineTransform(sourceTransform.position, sourceTransform.rotation);
                var srcOffset = AffineTransform.identity;
                var tmp = srcTx.InverseMul(drivenTx);

                if (data.maintainPositionOffset)
                    srcOffset.translation = tmp.translation;
                if (data.maintainRotationOffset)
                    srcOffset.rotation = tmp.rotation;

                job.sourceOffsets[i] = srcOffset;
            }

            job.positionAxesMask = new Vector3(
                System.Convert.ToSingle(data.constrainedPositionXAxis),
                System.Convert.ToSingle(data.constrainedPositionYAxis),
                System.Convert.ToSingle(data.constrainedPositionZAxis)
                );
            job.rotationAxesMask = new Vector3(
                System.Convert.ToSingle(data.constrainedRotationXAxis),
                System.Convert.ToSingle(data.constrainedRotationYAxis),
                System.Convert.ToSingle(data.constrainedRotationZAxis)
                );

            return job;
        }

        public override void Destroy(MultiParentConstraintJob job)
        {
            job.sourceTransforms.Dispose();
            job.sourceWeights.Dispose();
            job.sourceOffsets.Dispose();
            job.weightBuffer.Dispose();
        }
    }
}
