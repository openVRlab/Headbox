
namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct MultiPositionConstraintData : IAnimationJobData, IMultiPositionConstraintData
    {
        [SerializeField] Transform m_ConstrainedObject;

        [SyncSceneToStream, SerializeField, Range(0, 1)] WeightedTransformArray m_SourceObjects;
        [SyncSceneToStream, SerializeField] Vector3 m_Offset;

        [NotKeyable, SerializeField] Vector3Bool m_ConstrainedAxes;
        [NotKeyable, SerializeField] bool m_MaintainOffset;

        public Transform constrainedObject { get => m_ConstrainedObject; set => m_ConstrainedObject = value; }

        public WeightedTransformArray sourceObjects
        {
            get => m_SourceObjects;
            set => m_SourceObjects = value;
        }

        public bool maintainOffset { get => m_MaintainOffset; set => m_MaintainOffset = value; }
        public Vector3 offset { get => m_Offset; set => m_Offset = value; }

        public bool constrainedXAxis { get => m_ConstrainedAxes.x; set => m_ConstrainedAxes.x = value; }
        public bool constrainedYAxis { get => m_ConstrainedAxes.y; set => m_ConstrainedAxes.y = value; }
        public bool constrainedZAxis { get => m_ConstrainedAxes.z; set => m_ConstrainedAxes.z = value; }

        string IMultiPositionConstraintData.offsetVector3Property => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_Offset));
        string IMultiPositionConstraintData.sourceObjectsProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_SourceObjects));

        bool IAnimationJobData.IsValid()
        {
            if (m_ConstrainedObject == null || m_SourceObjects.Count == 0)
                return false;

            foreach (var src in m_SourceObjects)
                if (src.transform == null)
                    return false;

            return true;
        }

        void IAnimationJobData.SetDefaultValues()
        {
            m_ConstrainedObject = null;
            m_ConstrainedAxes = new Vector3Bool(true);
            m_SourceObjects.Clear();
            m_MaintainOffset = true;
            m_Offset = Vector3.zero;
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Multi-Position Constraint")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/MultiPositionConstraint.html")]
    public class MultiPositionConstraint : RigConstraint<
        MultiPositionConstraintJob,
        MultiPositionConstraintData,
        MultiPositionConstraintJobBinder<MultiPositionConstraintData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
        [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif
    }
}
