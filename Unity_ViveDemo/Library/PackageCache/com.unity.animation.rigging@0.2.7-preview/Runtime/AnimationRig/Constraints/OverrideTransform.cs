namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct OverrideTransformData : IAnimationJobData, IOverrideTransformData
    {
        [System.Serializable]
        public enum Space
        {
            World = OverrideTransformJob.Space.World,
            Local = OverrideTransformJob.Space.Local,
            Pivot = OverrideTransformJob.Space.Pivot
        }

        [SerializeField] Transform m_ConstrainedObject;

        [SyncSceneToStream, SerializeField] Transform m_OverrideSource;
        [SyncSceneToStream, SerializeField] Vector3 m_OverridePosition;
        [SyncSceneToStream, SerializeField] Vector3 m_OverrideRotation;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_PositionWeight;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_RotationWeight;

        [NotKeyable, SerializeField] Space m_Space;

        public Transform constrainedObject { get => m_ConstrainedObject; set => m_ConstrainedObject = value; }
        public Transform sourceObject { get => m_OverrideSource; set => m_OverrideSource = value; }
        public Space space { get => m_Space; set => m_Space = value; }
        public Vector3 position { get => m_OverridePosition; set => m_OverridePosition = value; }
        public Vector3 rotation { get => m_OverrideRotation; set => m_OverrideRotation = value; }
        public float positionWeight { get => m_PositionWeight; set => m_PositionWeight = Mathf.Clamp01(value); }
        public float rotationWeight { get => m_RotationWeight; set => m_RotationWeight = Mathf.Clamp01(value); }

        int IOverrideTransformData.space => (int)m_Space;
        string IOverrideTransformData.positionWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_PositionWeight));
        string IOverrideTransformData.rotationWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_RotationWeight));
        string IOverrideTransformData.positionVector3Property => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_OverridePosition));
        string IOverrideTransformData.rotationVector3Property => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_OverrideRotation));

        bool IAnimationJobData.IsValid() => m_ConstrainedObject != null;

        void IAnimationJobData.SetDefaultValues()
        {
            m_ConstrainedObject = null;
            m_OverrideSource = null;
            m_OverridePosition = Vector3.zero;
            m_OverrideRotation = Vector3.zero;
            m_Space = Space.Pivot;
            m_PositionWeight = 1f;
            m_RotationWeight = 1f;
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Override Transform")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/OverrideTransform.html")]
    public class OverrideTransform : RigConstraint<
        OverrideTransformJob,
        OverrideTransformData,
        OverrideTransformJobBinder<OverrideTransformData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
        [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif
    }
}
