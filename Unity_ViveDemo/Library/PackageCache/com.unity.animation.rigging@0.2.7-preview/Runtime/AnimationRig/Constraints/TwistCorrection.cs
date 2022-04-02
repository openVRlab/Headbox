
namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct TwistCorrectionData : IAnimationJobData, ITwistCorrectionData
    {
        public enum Axis { X, Y ,Z }

        [SyncSceneToStream, SerializeField] Transform m_Source;

        [NotKeyable, SerializeField] Axis m_TwistAxis;
        [SyncSceneToStream, SerializeField, Range(-1, 1)] WeightedTransformArray m_TwistNodes;

        public Transform sourceObject { get => m_Source; set => m_Source = value; }

        public WeightedTransformArray twistNodes
        {
            get => m_TwistNodes;
            set => m_TwistNodes = value;
        }

        public Axis twistAxis { get => m_TwistAxis; set => m_TwistAxis = value; }

        Transform ITwistCorrectionData.source => m_Source.transform;
        Vector3 ITwistCorrectionData.twistAxis => Convert(m_TwistAxis);

        string ITwistCorrectionData.twistNodesProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_TwistNodes));

        static Vector3 Convert(Axis axis)
        {
            if (axis == Axis.X)
                return Vector3.right;

            if (axis == Axis.Y)
                return Vector3.up;

            return Vector3.forward;
        }

        bool IAnimationJobData.IsValid()
        {
            if (m_Source.transform == null)
                return false;

            for (int i = 0; i < m_TwistNodes.Count; ++i)
                if (m_TwistNodes[i].transform == null)
                    return false;

            return true;
        }

        void IAnimationJobData.SetDefaultValues()
        {
            m_Source = null;
            m_TwistAxis = Axis.X;
            m_TwistNodes.Clear();
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Twist Correction")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/TwistCorrection.html")]
    public class TwistCorrection : RigConstraint<
        TwistCorrectionJob,
        TwistCorrectionData,
        TwistCorrectionJobBinder<TwistCorrectionData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_TwistNodesGUIToggle;
    #endif

        void OnValidate()
        {
        }
    }
}
