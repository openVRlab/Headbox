namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct DampedTransformData : IAnimationJobData, IDampedTransformData
    {
        [SerializeField] Transform m_ConstrainedObject;

        [SyncSceneToStream, SerializeField] Transform m_Source;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_DampPosition;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_DampRotation;

        [NotKeyable, SerializeField] bool m_MaintainAim;

        public Transform constrainedObject { get => m_ConstrainedObject; set => m_ConstrainedObject = value; }
        public Transform sourceObject { get => m_Source; set => m_Source = value; }
        public float dampPosition { get => m_DampPosition; set => m_DampPosition = Mathf.Clamp01(value); }
        public float dampRotation { get => m_DampRotation; set => m_DampRotation = Mathf.Clamp01(value); }
        public bool maintainAim { get => m_MaintainAim; set => m_MaintainAim = value; }

        string IDampedTransformData.dampPositionFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_DampPosition));
        string IDampedTransformData.dampRotationFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_DampRotation));

        bool IAnimationJobData.IsValid() => !(m_ConstrainedObject == null || m_Source == null);

        void IAnimationJobData.SetDefaultValues()
        {
            m_ConstrainedObject = null;
            m_Source = null;
            m_DampPosition = 0.5f;
            m_DampRotation = 0.5f;
            m_MaintainAim = true;
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Damped Transform")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/DampedTransform.html")]
    public class DampedTransform : RigConstraint<
        DampedTransformJob,
        DampedTransformData,
        DampedTransformJobBinder<DampedTransformData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
        [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif
    }
}
