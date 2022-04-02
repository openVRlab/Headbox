namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    public interface IRigConstraint
    {
        bool IsValid();

        IAnimationJob CreateJob(Animator animator);
        void UpdateJob(IAnimationJob job);
        void DestroyJob(IAnimationJob job);

        IAnimationJobData data { get; }
        IAnimationJobBinder binder { get; }
        float weight { get; set; }
    }
}
