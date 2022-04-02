using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct ChainIKConstraintJob : IWeightedAnimationJob
    {
        public NativeArray<ReadWriteTransformHandle> chain;
        public ReadOnlyTransformHandle target;
        public AffineTransform targetOffset;

        public NativeArray<float> linkLengths;
        public NativeArray<Vector3> linkPositions;

        public FloatProperty chainRotationWeight;
        public FloatProperty tipRotationWeight;

        public CacheIndex toleranceIdx;
        public CacheIndex maxIterationsIdx;
        public AnimationJobCache cache;

        public float maxReach;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                for (int i = 0; i < chain.Length; ++i)
                {
                    var handle = chain[i];
                    linkPositions[i] = handle.GetPosition(stream);
                    chain[i] = handle;
                }

                int tipIndex = chain.Length - 1;
                if (AnimationRuntimeUtils.SolveFABRIK(ref linkPositions, ref linkLengths, target.GetPosition(stream) + targetOffset.translation,
                    cache.GetRaw(toleranceIdx), maxReach, (int)cache.GetRaw(maxIterationsIdx)))
                {
                    var chainRWeight = chainRotationWeight.Get(stream) * w;
                    for (int i = 0; i < tipIndex; ++i)
                    {
                        var prevDir = chain[i + 1].GetPosition(stream) - chain[i].GetPosition(stream);
                        var newDir = linkPositions[i + 1] - linkPositions[i];
                        var rot = chain[i].GetRotation(stream);
                        chain[i].SetRotation(stream, Quaternion.Lerp(rot, QuaternionExt.FromToRotation(prevDir, newDir) * rot, chainRWeight));
                    }
                }

                chain[tipIndex].SetRotation(
                    stream,
                    Quaternion.Lerp(
                        chain[tipIndex].GetRotation(stream),
                        target.GetRotation(stream) * targetOffset.rotation,
                        tipRotationWeight.Get(stream) * w
                        )
                    );
            }
            else
            {
                for (int i = 0; i < chain.Length; ++i)
                    AnimationRuntimeUtils.PassThrough(stream, chain[i]);
            }
        }
    }

    public interface IChainIKConstraintData
    {
        Transform root { get; }
        Transform tip { get; }
        Transform target { get; }

        int maxIterations { get; }
        float tolerance { get; }
        bool maintainTargetPositionOffset { get; }
        bool maintainTargetRotationOffset { get; }

        string chainRotationWeightFloatProperty { get; }
        string tipRotationWeightFloatProperty { get; }
    }

    public class ChainIKConstraintJobBinder<T> : AnimationJobBinder<ChainIKConstraintJob, T>
        where T : struct, IAnimationJobData, IChainIKConstraintData
    {
        public override ChainIKConstraintJob Create(Animator animator, ref T data, Component component)
        {
            List<Transform> chain = new List<Transform>();
            Transform tmp = data.tip;
            while (tmp != data.root)
            {
                chain.Add(tmp);
                tmp = tmp.parent;
            }
            chain.Add(data.root);
            chain.Reverse();

            var job = new ChainIKConstraintJob();
            job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.linkLengths = new NativeArray<float>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.linkPositions = new NativeArray<Vector3>(chain.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.maxReach = 0f;

            int tipIndex = chain.Count - 1;
            for (int i = 0; i < chain.Count; ++i)
            {
                job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
                job.linkLengths[i] = (i != tipIndex) ? Vector3.Distance(chain[i].position, chain[i + 1].position) : 0f;
                job.maxReach += job.linkLengths[i];
            }

            job.target = ReadOnlyTransformHandle.Bind(animator, data.target);
            job.targetOffset = AffineTransform.identity;
            if (data.maintainTargetPositionOffset)
                job.targetOffset.translation = data.tip.position - data.target.position;
            if (data.maintainTargetRotationOffset)
                job.targetOffset.rotation = Quaternion.Inverse(data.target.rotation) * data.tip.rotation;

            job.chainRotationWeight = FloatProperty.Bind(animator, component, data.chainRotationWeightFloatProperty);
            job.tipRotationWeight = FloatProperty.Bind(animator, component, data.tipRotationWeightFloatProperty);
 
            var cacheBuilder = new AnimationJobCacheBuilder();
            job.maxIterationsIdx = cacheBuilder.Add(data.maxIterations);
            job.toleranceIdx = cacheBuilder.Add(data.tolerance);
            job.cache = cacheBuilder.Build();

            return job;
        }

        public override void Destroy(ChainIKConstraintJob job)
        {
            job.chain.Dispose();
            job.linkLengths.Dispose();
            job.linkPositions.Dispose();
            job.cache.Dispose();
        }

        public override void Update(ChainIKConstraintJob job, ref T data)
        {
            job.cache.SetRaw(data.maxIterations, job.maxIterationsIdx);
            job.cache.SetRaw(data.tolerance, job.toleranceIdx);
        }
    }
}
