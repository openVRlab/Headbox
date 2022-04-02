namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    public class RigConstraint<TJob, TData, TBinder> : MonoBehaviour, IRigConstraint
        where TJob    : struct, IWeightedAnimationJob
        where TData   : struct, IAnimationJobData
        where TBinder : AnimationJobBinder<TJob, TData>, new()
    {
        [SerializeField, Range(0f, 1f)]
        protected float m_Weight = 1f;

        [SerializeField]
        protected TData m_Data;

        static readonly TBinder s_Binder = new TBinder();

        public void Reset()
        {
            m_Weight = 1f;
            m_Data.SetDefaultValues();
        }

        public bool IsValid() => m_Data.IsValid();

        public ref TData data => ref m_Data;

        public float weight { get => m_Weight; set => m_Weight = Mathf.Clamp01(value); }

        public IAnimationJob CreateJob(Animator animator)
        {
            TJob job = s_Binder.Create(animator, ref m_Data, this);

            // Bind constraint job weight property
            job.jobWeight = FloatProperty.BindCustom(
                animator,
                PropertyUtils.ConstructCustomPropertyName(this, ConstraintProperties.s_Weight)
                );

            return job;
        }

        public void DestroyJob(IAnimationJob job) => s_Binder.Destroy((TJob)job);

        public void UpdateJob(IAnimationJob job) => s_Binder.Update((TJob)job, ref m_Data);

        IAnimationJobBinder IRigConstraint.binder => s_Binder;
        IAnimationJobData IRigConstraint.data => m_Data;
    }
}
