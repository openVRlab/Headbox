namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct OverrideTransformJob : IWeightedAnimationJob
    {
        public enum Space
        {
            World = 0,
            Local = 1,
            Pivot = 2
        }

        public ReadWriteTransformHandle driven;
        public ReadOnlyTransformHandle source;
        public AffineTransform sourceInvLocalBindTx;

        public Quaternion sourceToWorldRot;
        public Quaternion sourceToLocalRot;
        public Quaternion sourceToPivotRot;

        public CacheIndex spaceIdx;
        public CacheIndex sourceToCurrSpaceRotIdx;

        public Vector3Property position;
        public Vector3Property rotation;
        public FloatProperty positionWeight;
        public FloatProperty rotationWeight;

        public AnimationJobCache cache;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                AffineTransform overrideTx;
                if (source.IsValid(stream))
                {
                    source.GetLocalTRS(stream, out Vector3 srcLPos, out Quaternion srcLRot, out _);
                    var sourceLocalTx = new AffineTransform(srcLPos, srcLRot);
                    var sourceToSpaceRot = cache.Get<Quaternion>(sourceToCurrSpaceRotIdx);
                    overrideTx = Quaternion.Inverse(sourceToSpaceRot) * (sourceInvLocalBindTx * sourceLocalTx) * sourceToSpaceRot;
                }
                else
                    overrideTx = new AffineTransform(position.Get(stream), Quaternion.Euler(rotation.Get(stream)));

                Space overrideSpace = (Space)cache.GetRaw(spaceIdx);
                var posW = positionWeight.Get(stream) * w;
                var rotW = rotationWeight.Get(stream) * w;
                switch (overrideSpace)
                {
                    case Space.World:
                        {
                            driven.GetGlobalTR(stream, out Vector3 drivenWPos, out Quaternion drivenWRot);
                            driven.SetGlobalTR(
                                stream,
                                Vector3.Lerp(drivenWPos, overrideTx.translation, posW),
                                Quaternion.Lerp(drivenWRot, overrideTx.rotation, rotW)
                                );
                        }
                        break;
                    case Space.Local:
                        {
                            driven.GetLocalTRS(stream, out Vector3 drivenLPos, out Quaternion drivenLRot, out Vector3 drivenLScale);
                            driven.SetLocalTRS(
                                stream,
                                Vector3.Lerp(drivenLPos, overrideTx.translation, posW),
                                Quaternion.Lerp(drivenLRot, overrideTx.rotation, rotW),
                                drivenLScale
                                );
                        }
                        break;
                    case Space.Pivot:
                        {
                            driven.GetLocalTRS(stream, out Vector3 drivenLPos, out Quaternion drivenLRot, out Vector3 drivenLScale);
                            var drivenLocalTx = new AffineTransform(drivenLPos, drivenLRot);
                            overrideTx = drivenLocalTx * overrideTx;

                            driven.SetLocalTRS(
                                stream,
                                Vector3.Lerp(drivenLocalTx.translation, overrideTx.translation, posW),
                                Quaternion.Lerp(drivenLocalTx.rotation, overrideTx.rotation, rotW),
                                drivenLScale
                                );
                        }
                        break;
                    default:
                        break;
                }
            }
            else
                AnimationRuntimeUtils.PassThrough(stream, driven);
        }

        public void UpdateSpace(int space)
        {
            if ((int)cache.GetRaw(spaceIdx) == space)
                return;

            cache.SetRaw(space, spaceIdx);

            Space currSpace = (Space)space;
            if (currSpace == Space.Pivot)
                cache.Set(sourceToPivotRot, sourceToCurrSpaceRotIdx);
            else if (currSpace == Space.Local)
                cache.Set(sourceToLocalRot, sourceToCurrSpaceRotIdx);
            else
                cache.Set(sourceToWorldRot, sourceToCurrSpaceRotIdx);
        }
    }

    public interface IOverrideTransformData
    {
        Transform constrainedObject { get; }
        Transform sourceObject { get; }
        int space { get; }

        string positionWeightFloatProperty { get; }
        string rotationWeightFloatProperty { get; }
        string positionVector3Property { get; }
        string rotationVector3Property { get; }
    }

    public class OverrideTransformJobBinder<T> : AnimationJobBinder<OverrideTransformJob, T>
        where T : struct, IAnimationJobData, IOverrideTransformData
    {
        public override OverrideTransformJob Create(Animator animator, ref T data, Component component)
        {
            var job = new OverrideTransformJob();
            var cacheBuilder = new AnimationJobCacheBuilder();

            job.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);

            if (data.sourceObject != null)
            {
                // Cache source to possible space rotation offsets (world, local and pivot)
                // at bind time so we can switch dynamically between them at runtime.

                job.source = ReadOnlyTransformHandle.Bind(animator, data.sourceObject);
                var sourceLocalTx = new AffineTransform(data.sourceObject.localPosition, data.sourceObject.localRotation);
                job.sourceInvLocalBindTx = sourceLocalTx.Inverse();

                var sourceWorldTx = new AffineTransform(data.sourceObject.position, data.sourceObject.rotation);
                var drivenWorldTx = new AffineTransform(data.constrainedObject.position, data.constrainedObject.rotation);
                job.sourceToWorldRot = sourceWorldTx.Inverse().rotation;
                job.sourceToPivotRot = sourceWorldTx.InverseMul(drivenWorldTx).rotation;

                var drivenParent = data.constrainedObject.parent;
                if (drivenParent != null)
                {
                    var drivenParentWorldTx = new AffineTransform(drivenParent.position, drivenParent.rotation); 
                    job.sourceToLocalRot = sourceWorldTx.InverseMul(drivenParentWorldTx).rotation;
                }
                else
                    job.sourceToLocalRot = job.sourceToPivotRot;
            }

            job.spaceIdx = cacheBuilder.Add(data.space);
            if (data.space == (int)OverrideTransformJob.Space.Pivot)
                job.sourceToCurrSpaceRotIdx = cacheBuilder.Add(job.sourceToPivotRot);
            else if (data.space == (int)OverrideTransformJob.Space.Local)
                job.sourceToCurrSpaceRotIdx = cacheBuilder.Add(job.sourceToLocalRot);
            else
                job.sourceToCurrSpaceRotIdx = cacheBuilder.Add(job.sourceToWorldRot);

            job.position = Vector3Property.Bind(animator, component, data.positionVector3Property);
            job.rotation = Vector3Property.Bind(animator, component, data.rotationVector3Property);
            job.positionWeight = FloatProperty.Bind(animator, component, data.positionWeightFloatProperty);
            job.rotationWeight = FloatProperty.Bind(animator, component, data.rotationWeightFloatProperty);

            job.cache = cacheBuilder.Build();

            return job;
        }

        public override void Destroy(OverrideTransformJob job)
        {
            job.cache.Dispose();
        }

        public override void Update(OverrideTransformJob job, ref T data)
        {
            job.UpdateSpace(data.space);
        }
    }
}