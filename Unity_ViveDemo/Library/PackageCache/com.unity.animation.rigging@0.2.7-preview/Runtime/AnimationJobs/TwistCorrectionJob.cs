using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct TwistCorrectionJob : IWeightedAnimationJob
    {
        public ReadOnlyTransformHandle source;
        public Quaternion sourceInverseBindRotation;
        public Vector3 axisMask;

        public NativeArray<ReadWriteTransformHandle> twistTransforms;
        public NativeArray<PropertyStreamHandle> twistWeights;
        public NativeArray<Quaternion> twistBindRotations;

        public NativeArray<float> weightBuffer;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                if (twistTransforms.Length == 0)
                    return;

                AnimationStreamHandleUtility.ReadFloats(stream, twistWeights, weightBuffer);

                Quaternion twistRot = TwistRotation(axisMask, sourceInverseBindRotation * source.GetLocalRotation(stream));
                Quaternion invTwistRot = Quaternion.Inverse(twistRot);
                for (int i = 0; i < twistTransforms.Length; ++i)
                {
                    ReadWriteTransformHandle twistTransform = twistTransforms[i];

                    float twistWeight = Mathf.Clamp(weightBuffer[i], -1f, 1f);
                    Quaternion rot = Quaternion.Lerp(Quaternion.identity, Mathf.Sign(twistWeight) < 0f ? invTwistRot : twistRot, Mathf.Abs(twistWeight));
                    twistTransform.SetLocalRotation(stream, Quaternion.Lerp(twistBindRotations[i], rot, w));

                    // Required to update handles with binding info.
                    twistTransforms[i] = twistTransform;
                }
            }
            else
            {
                for (int i = 0; i < twistTransforms.Length; ++i)
                    AnimationRuntimeUtils.PassThrough(stream, twistTransforms[i]);
            }
        }

        static Quaternion TwistRotation(Vector3 axis, Quaternion rot)
        {
            return new Quaternion(axis.x * rot.x, axis.y * rot.y, axis.z * rot.z, rot.w);
        }
    }

    public interface ITwistCorrectionData
    {
        Transform source { get; }
        WeightedTransformArray twistNodes { get; }
        Vector3 twistAxis { get; }

        string twistNodesProperty { get; }
    }

    public class TwistCorrectionJobBinder<T> : AnimationJobBinder<TwistCorrectionJob, T>
        where T : struct, IAnimationJobData, ITwistCorrectionData
    {
        public override TwistCorrectionJob Create(Animator animator, ref T data, Component component)
        {
            var job = new TwistCorrectionJob();

            job.source = ReadOnlyTransformHandle.Bind(animator, data.source);
            job.sourceInverseBindRotation = Quaternion.Inverse(data.source.localRotation);
            job.axisMask = data.twistAxis;

            WeightedTransformArray twistNodes = data.twistNodes;

            WeightedTransformArrayBinder.BindReadWriteTransforms(animator, component, twistNodes, out job.twistTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, twistNodes, data.twistNodesProperty, out job.twistWeights);

            job.weightBuffer = new NativeArray<float>(twistNodes.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            job.twistBindRotations = new NativeArray<Quaternion>(twistNodes.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < twistNodes.Count; ++i)
            {
                var sourceTransform = twistNodes[i].transform;
                job.twistBindRotations[i] = sourceTransform.localRotation;
            }

            return job;
        }

        public override void Destroy(TwistCorrectionJob job)
        {
            job.twistTransforms.Dispose();
            job.twistWeights.Dispose();
            job.twistBindRotations.Dispose();
            job.weightBuffer.Dispose();
        }
    }
}
