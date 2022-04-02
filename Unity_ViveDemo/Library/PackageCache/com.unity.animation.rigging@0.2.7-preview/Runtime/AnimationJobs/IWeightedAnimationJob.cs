namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    public interface IWeightedAnimationJob : IAnimationJob
    {
        FloatProperty jobWeight { get; set; }
    }
}
