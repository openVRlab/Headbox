namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct DampedTransformJob : IWeightedAnimationJob
    {
        const float k_FixedDt = 0.01667f; // 60Hz simulation step
        const float k_DampFactor = 40f;

        public ReadWriteTransformHandle driven;
        public ReadOnlyTransformHandle source;
        public AffineTransform localBindTx;

        public Vector3 aimBindAxis;
        public AffineTransform prevDrivenTx;

        public FloatProperty dampPosition;
        public FloatProperty dampRotation;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            float streamDt = Mathf.Abs(stream.deltaTime);
            driven.GetGlobalTR(stream, out Vector3 drivenPos, out Quaternion drivenRot);

            if (w > 0f && streamDt > 0f)
            {
                source.GetGlobalTR(stream, out Vector3 sourcePos, out Quaternion sourceRot);
                var sourceTx = new AffineTransform(sourcePos, sourceRot);
                var targetTx = sourceTx * localBindTx;
                targetTx.translation = Vector3.Lerp(drivenPos, targetTx.translation, w);
                targetTx.rotation = Quaternion.Lerp(drivenRot, targetTx.rotation, w);

                var dampPosW = AnimationRuntimeUtils.Square(1f - dampPosition.Get(stream));
                var dampRotW = AnimationRuntimeUtils.Square(1f - dampRotation.Get(stream));
                bool doAimAjustements = Vector3.Dot(aimBindAxis, aimBindAxis) > 0f;

                while (streamDt > 0f)
                {
                    float factoredDt = k_DampFactor * Mathf.Min(k_FixedDt, streamDt);

                    prevDrivenTx.translation +=
                        (targetTx.translation - prevDrivenTx.translation) * dampPosW * factoredDt;

                    prevDrivenTx.rotation *= Quaternion.Lerp(
                        Quaternion.identity,
                        Quaternion.Inverse(prevDrivenTx.rotation) * targetTx.rotation,
                        dampRotW * factoredDt
                        );

                    if (doAimAjustements)
                    {
                        var fromDir = prevDrivenTx.rotation * aimBindAxis;
                        var toDir = sourceTx.translation - prevDrivenTx.translation;
                        prevDrivenTx.rotation =
                            Quaternion.AngleAxis(Vector3.Angle(fromDir, toDir), Vector3.Cross(fromDir, toDir).normalized) * prevDrivenTx.rotation;
                    }

                    streamDt -= k_FixedDt;
                }

                driven.SetGlobalTR(stream, prevDrivenTx.translation, prevDrivenTx.rotation);
            }
            else
            {
                prevDrivenTx.Set(drivenPos, drivenRot);
                AnimationRuntimeUtils.PassThrough(stream, driven);
            }
        }
    }

    public interface IDampedTransformData
    {
        Transform constrainedObject { get; }
        Transform sourceObject { get; }
        bool maintainAim { get; }

        string dampPositionFloatProperty { get; }
        string dampRotationFloatProperty { get; }
    }

    public class DampedTransformJobBinder<T> : AnimationJobBinder<DampedTransformJob, T>
        where T : struct, IAnimationJobData, IDampedTransformData
    {
        public override DampedTransformJob Create(Animator animator, ref T data, Component component)
        {
            var job = new DampedTransformJob();

            job.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
            job.source = ReadOnlyTransformHandle.Bind(animator, data.sourceObject);

            var drivenTx = new AffineTransform(data.constrainedObject.position, data.constrainedObject.rotation);
            var sourceTx = new AffineTransform(data.sourceObject.position, data.sourceObject.rotation);

            job.localBindTx = sourceTx.InverseMul(drivenTx);
            job.prevDrivenTx = drivenTx;

            job.dampPosition = FloatProperty.Bind(animator, component, data.dampPositionFloatProperty);
            job.dampRotation = FloatProperty.Bind(animator, component, data.dampRotationFloatProperty);

            if (data.maintainAim && AnimationRuntimeUtils.SqrDistance(data.constrainedObject.position, data.sourceObject.position) > 0f)
                job.aimBindAxis = Quaternion.Inverse(data.constrainedObject.rotation) * (sourceTx.translation - drivenTx.translation).normalized;
            else
                job.aimBindAxis = Vector3.zero;

            return job;
        }

        public override void Destroy(DampedTransformJob job)
        {
        }
    }
}
