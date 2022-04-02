
namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct MultiParentConstraintData : IAnimationJobData, IMultiParentConstraintData
    {
        [SerializeField] Transform m_ConstrainedObject;

        [SerializeField, SyncSceneToStream, Range(0, 1)] private WeightedTransformArray m_SourceObjects;

        [NotKeyable, SerializeField] Vector3Bool m_ConstrainedPositionAxes;
        [NotKeyable, SerializeField] Vector3Bool m_ConstrainedRotationAxes;
        [NotKeyable, SerializeField] bool m_MaintainPositionOffset;
        [NotKeyable, SerializeField] bool m_MaintainRotationOffset;

        public Transform constrainedObject { get => m_ConstrainedObject; set => m_ConstrainedObject = value; }

        public WeightedTransformArray sourceObjects
        {
            get => m_SourceObjects;
            set => m_SourceObjects = value;
        }

        public bool maintainPositionOffset { get => m_MaintainPositionOffset; set => m_MaintainPositionOffset = value; }
        public bool maintainRotationOffset { get => m_MaintainRotationOffset; set => m_MaintainRotationOffset = value; }

        public bool constrainedPositionXAxis { get => m_ConstrainedPositionAxes.x; set => m_ConstrainedPositionAxes.x = value; }
        public bool constrainedPositionYAxis { get => m_ConstrainedPositionAxes.y; set => m_ConstrainedPositionAxes.y = value; }
        public bool constrainedPositionZAxis { get => m_ConstrainedPositionAxes.z; set => m_ConstrainedPositionAxes.z = value; }
        public bool constrainedRotationXAxis { get => m_ConstrainedRotationAxes.x; set => m_ConstrainedRotationAxes.x = value; }
        public bool constrainedRotationYAxis { get => m_ConstrainedRotationAxes.y; set => m_ConstrainedRotationAxes.y = value; }
        public bool constrainedRotationZAxis { get => m_ConstrainedRotationAxes.z; set => m_ConstrainedRotationAxes.z = value; }

        string IMultiParentConstraintData.sourceObjectsProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_SourceObjects));

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
            m_ConstrainedPositionAxes = new Vector3Bool(true);
            m_ConstrainedRotationAxes = new Vector3Bool(true);
            m_SourceObjects.Clear();
            m_MaintainPositionOffset = true;
            m_MaintainRotationOffset = true;
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Multi-Parent Constraint")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/MultiParentConstraint.html")]
    public class MultiParentConstraint : RigConstraint<
        MultiParentConstraintJob,
        MultiParentConstraintData,
        MultiParentConstraintJobBinder<MultiParentConstraintData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
        [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif
    }
}
