namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct BlendConstraintData : IAnimationJobData, IBlendConstraintData
    {
        [SerializeField] Transform m_ConstrainedObject;

        [SyncSceneToStream, SerializeField] Transform m_SourceA;
        [SyncSceneToStream, SerializeField] Transform m_SourceB;
        [SyncSceneToStream, SerializeField] bool m_BlendPosition;
        [SyncSceneToStream, SerializeField] bool m_BlendRotation;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_PositionWeight;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_RotationWeight;

        [NotKeyable, SerializeField] bool m_MaintainPositionOffsets;
        [NotKeyable, SerializeField] bool m_MaintainRotationOffsets;

        public Transform constrainedObject { get => m_ConstrainedObject; set => m_ConstrainedObject = value; }
        public Transform sourceObjectA { get => m_SourceA; set => m_SourceA = value; }
        public Transform sourceObjectB { get => m_SourceB; set => m_SourceB = value; }
        public bool blendPosition { get => m_BlendPosition; set => m_BlendPosition = value; }
        public bool blendRotation { get => m_BlendRotation; set => m_BlendRotation = value; }
        public float positionWeight { get => m_PositionWeight; set => m_PositionWeight = Mathf.Clamp01(value); }
        public float rotationWeight { get => m_RotationWeight; set => m_RotationWeight = Mathf.Clamp01(value); }
        public bool maintainPositionOffsets { get => m_MaintainPositionOffsets; set => m_MaintainPositionOffsets = value; }
        public bool maintainRotationOffsets { get => m_MaintainRotationOffsets; set => m_MaintainRotationOffsets = value; }

        string IBlendConstraintData.blendPositionBoolProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_BlendPosition));
        string IBlendConstraintData.blendRotationBoolProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_BlendRotation));
        string IBlendConstraintData.positionWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_PositionWeight));
        string IBlendConstraintData.rotationWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_RotationWeight));

        bool IAnimationJobData.IsValid() => !(m_ConstrainedObject == null || m_SourceA == null || m_SourceB == null);

        void IAnimationJobData.SetDefaultValues()
        {
            m_ConstrainedObject = null;
            m_SourceA = null;
            m_SourceB = null;
            m_BlendPosition = true;
            m_BlendRotation = true;
            m_PositionWeight = 0.5f;
            m_RotationWeight = 0.5f;
            m_MaintainPositionOffsets = false;
            m_MaintainRotationOffsets = false;
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Blend Constraint")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/BlendConstraint.html")]
    public class BlendConstraint : RigConstraint<
        BlendConstraintJob,
        BlendConstraintData,
        BlendConstraintJobBinder<BlendConstraintData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
        [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif
    }
}
