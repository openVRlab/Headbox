namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [Unity.Burst.BurstCompile]
    public struct BlendConstraintJob : IWeightedAnimationJob
    {
        public const int k_BlendTranslationMask = 1 << 0;
        public const int k_BlendRotationMask = 1 << 1;

        public ReadWriteTransformHandle driven;
        public ReadOnlyTransformHandle sourceA;
        public ReadOnlyTransformHandle sourceB;
        public AffineTransform sourceAOffset;
        public AffineTransform sourceBOffset;

        public BoolProperty blendPosition;
        public BoolProperty blendRotation;
        public FloatProperty positionWeight;
        public FloatProperty rotationWeight;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                if (blendPosition.Get(stream))
                {
                    Vector3 posBlend = Vector3.Lerp(
                        sourceA.GetPosition(stream) + sourceAOffset.translation,
                        sourceB.GetPosition(stream) + sourceBOffset.translation,
                        positionWeight.Get(stream)
                        );
                    driven.SetPosition(stream, Vector3.Lerp(driven.GetPosition(stream), posBlend, w));
                }
                else
                    driven.SetLocalPosition(stream, driven.GetLocalPosition(stream));

                if (blendRotation.Get(stream))
                {
                    Quaternion rotBlend = Quaternion.Lerp(
                        sourceA.GetRotation(stream) * sourceAOffset.rotation,
                        sourceB.GetRotation(stream) * sourceBOffset.rotation,
                        rotationWeight.Get(stream)
                        );
                    driven.SetRotation(stream, Quaternion.Lerp(driven.GetRotation(stream), rotBlend, w));
                }
                else
                    driven.SetLocalRotation(stream, driven.GetLocalRotation(stream));
            }
            else
                AnimationRuntimeUtils.PassThrough(stream, driven);
        }
    }

    public interface IBlendConstraintData
    {
        Transform constrainedObject { get; }
        Transform sourceObjectA { get; }
        Transform sourceObjectB { get; }

        bool maintainPositionOffsets { get; }
        bool maintainRotationOffsets { get; }

        string blendPositionBoolProperty { get; }
        string blendRotationBoolProperty { get; }
        string positionWeightFloatProperty { get; }
        string rotationWeightFloatProperty { get; }
    }

    public class BlendConstraintJobBinder<T> : AnimationJobBinder<BlendConstraintJob, T>
        where T : struct, IAnimationJobData, IBlendConstraintData
    {
        public override BlendConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new BlendConstraintJob();
            
            job.driven = ReadWriteTransformHandle.Bind(animator, data.constrainedObject);
            job.sourceA = ReadOnlyTransformHandle.Bind(animator, data.sourceObjectA);
            job.sourceB = ReadOnlyTransformHandle.Bind(animator, data.sourceObjectB);
            
            job.sourceAOffset = job.sourceBOffset = AffineTransform.identity;
            if (data.maintainPositionOffsets)
            {
                var drivenPos = data.constrainedObject.position;
                job.sourceAOffset.translation = drivenPos - data.sourceObjectA.position;
                job.sourceBOffset.translation = drivenPos - data.sourceObjectB.position;
            }

            if (data.maintainRotationOffsets)
            {
                var drivenRot = data.constrainedObject.rotation;
                job.sourceAOffset.rotation = Quaternion.Inverse(data.sourceObjectA.rotation) * drivenRot;
                job.sourceBOffset.rotation = Quaternion.Inverse(data.sourceObjectB.rotation) * drivenRot;
            }

            job.blendPosition = BoolProperty.Bind(animator, component, data.blendPositionBoolProperty);
            job.blendRotation = BoolProperty.Bind(animator, component, data.blendRotationBoolProperty);
            job.positionWeight = FloatProperty.Bind(animator, component, data.positionWeightFloatProperty);
            job.rotationWeight = FloatProperty.Bind(animator, component, data.rotationWeightFloatProperty);

            return job;
        }

        public override void Destroy(BlendConstraintJob job)
        {
        }
    }
}