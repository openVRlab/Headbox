using System;

namespace UnityEngine.Animations.Rigging
{
    public interface ITransformProvider
    {
        Transform transform { get; set; }
    }

    public interface IWeightProvider
    {
        float weight { get; set; }
    }

    [System.Serializable]
    public struct WeightedTransform : ITransformProvider, IWeightProvider, IEquatable<WeightedTransform>
    {
        public Transform transform;
        public float weight;

        public WeightedTransform(Transform transform, float weight)
        {
            this.transform = transform;
            this.weight = Mathf.Clamp01(weight);
        }

        public static WeightedTransform Default(float weight) => new WeightedTransform(null, weight);

        public bool Equals(WeightedTransform other)
        {
            if (transform == other.transform && weight == other.weight)
                return true;

            return false;
        }

        Transform ITransformProvider.transform { get => transform; set => transform = value; }
        float IWeightProvider.weight { get => weight; set => weight = Mathf.Clamp01(value); }
    }
}
