namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct TwoBoneIKConstraintJob : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle root;
        public ReadWriteTransformHandle mid;
        public ReadWriteTransformHandle tip;

        public ReadOnlyTransformHandle hint;
        public ReadOnlyTransformHandle target;

        public AffineTransform targetOffset;
        public Vector2 linkLengths;

        public FloatProperty targetPositionWeight;
        public FloatProperty targetRotationWeight;
        public FloatProperty hintWeight;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                AnimationRuntimeUtils.SolveTwoBoneIK(
                    stream, root, mid, tip, target, hint,
                    targetPositionWeight.Get(stream) * w,
                    targetRotationWeight.Get(stream) * w,
                    hintWeight.Get(stream) * w,
                    linkLengths,
                    targetOffset
                    );
            }
            else
            {
                AnimationRuntimeUtils.PassThrough(stream, root);
                AnimationRuntimeUtils.PassThrough(stream, mid);
                AnimationRuntimeUtils.PassThrough(stream, tip);
            }
        }
    }

    public interface ITwoBoneIKConstraintData
    {
        Transform root { get; }
        Transform mid { get; }
        Transform tip { get; }
        Transform target { get; }
        Transform hint { get; }

        bool maintainTargetPositionOffset { get; }
        bool maintainTargetRotationOffset { get; }

        string targetPositionWeightFloatProperty { get; }
        string targetRotationWeightFloatProperty { get; }
        string hintWeightFloatProperty { get; }
    }

    public class TwoBoneIKConstraintJobBinder<T> : AnimationJobBinder<TwoBoneIKConstraintJob, T>
        where T : struct, IAnimationJobData, ITwoBoneIKConstraintData
    {
        public override TwoBoneIKConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new TwoBoneIKConstraintJob();

            job.root = ReadWriteTransformHandle.Bind(animator, data.root);
            job.mid = ReadWriteTransformHandle.Bind(animator, data.mid);
            job.tip = ReadWriteTransformHandle.Bind(animator, data.tip);
            job.target = ReadOnlyTransformHandle.Bind(animator, data.target);

            if (data.hint != null)
                job.hint = ReadOnlyTransformHandle.Bind(animator, data.hint);

            job.targetOffset = AffineTransform.identity;
            if (data.maintainTargetPositionOffset)
                job.targetOffset.translation = data.tip.position - data.target.position;
            if (data.maintainTargetRotationOffset)
                job.targetOffset.rotation = Quaternion.Inverse(data.target.rotation) * data.tip.rotation;

            job.linkLengths[0] = Vector3.Distance(data.root.position, data.mid.position);
            job.linkLengths[1] = Vector3.Distance(data.mid.position, data.tip.position);

            job.targetPositionWeight = FloatProperty.Bind(animator, component, data.targetPositionWeightFloatProperty);
            job.targetRotationWeight = FloatProperty.Bind(animator, component, data.targetRotationWeightFloatProperty);
            job.hintWeight = FloatProperty.Bind(animator, component, data.hintWeightFloatProperty);

            return job;
        }

        public override void Destroy(TwoBoneIKConstraintJob job)
        {
        }
    }
}
