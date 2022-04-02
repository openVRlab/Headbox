using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct MultiReferentialConstraintJob : IWeightedAnimationJob
    {
        public IntProperty driver;
        public NativeArray<ReadWriteTransformHandle> sources;
        public NativeArray<AffineTransform> sourceBindTx;
        public NativeArray<AffineTransform> offsetTx;
        public int prevDriverIdx;

        public FloatProperty jobWeight { get; set; }
    
        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                var driverIdx = driver.Get(stream);
                if (driverIdx != prevDriverIdx)
                    UpdateOffsets(driverIdx);

                sources[driverIdx].GetGlobalTR(stream, out Vector3 driverWPos, out Quaternion driverWRot);
                var driverTx = new AffineTransform(driverWPos, driverWRot);

                int offset = 0;
                for (int i = 0; i < sources.Length; ++i)
                {
                    if (i == driverIdx)
                        continue;

                    var tx = driverTx * offsetTx[offset];

                    var src = sources[i];
                    src.GetGlobalTR(stream, out Vector3 srcWPos, out Quaternion srcWRot);
                    src.SetGlobalTR(stream, Vector3.Lerp(srcWPos, tx.translation, w), Quaternion.Lerp(srcWRot, tx.rotation, w));
                    offset++;

                    sources[i] = src;
                }
            }
            else
            {
                for (int i = 0; i < sources.Length; ++i)
                    AnimationRuntimeUtils.PassThrough(stream, sources[i]);
            }
        }

        public void UpdateOffsets(int driver)
        {
            driver = Mathf.Clamp(driver, 0, sources.Length - 1);

            int offset = 0;
            var invDriverTx = sourceBindTx[driver].Inverse();
            for (int i = 0; i < sourceBindTx.Length; ++i)
            {
                if (i == driver)
                    continue;

                offsetTx[offset] = invDriverTx * sourceBindTx[i];
                offset++;
            }

            prevDriverIdx = driver;
        }
    }

    public interface IMultiReferentialConstraintData
    {
        int driverValue { get; }
        string driverIntProperty { get; }
        Transform[] sourceObjects { get; }
    }

    public class MultiReferentialConstraintJobBinder<T> : AnimationJobBinder<MultiReferentialConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiReferentialConstraintData
    {
        public override MultiReferentialConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiReferentialConstraintJob();

            var sources = data.sourceObjects;
            job.driver = IntProperty.Bind(animator, component, data.driverIntProperty);
            job.sources = new NativeArray<ReadWriteTransformHandle>(sources.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.sourceBindTx = new NativeArray<AffineTransform>(sources.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.offsetTx = new NativeArray<AffineTransform>(sources.Length - 1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < sources.Length; ++i)
            {
                job.sources[i] = ReadWriteTransformHandle.Bind(animator, sources[i].transform);
                job.sourceBindTx[i] = new AffineTransform(sources[i].position, sources[i].rotation);
            }
            
            job.UpdateOffsets(data.driverValue);

            return job;
        }

        public override void Destroy(MultiReferentialConstraintJob job)
        {
            job.sources.Dispose();
            job.sourceBindTx.Dispose();
            job.offsetTx.Dispose();
        }
    }
}