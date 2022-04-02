namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct TwoBoneIKConstraintData : IAnimationJobData, ITwoBoneIKConstraintData
    {
        [SerializeField] Transform m_Root;
        [SerializeField] Transform m_Mid;
        [SerializeField] Transform m_Tip;

        [SyncSceneToStream, SerializeField] Transform m_Target;
        [SyncSceneToStream, SerializeField] Transform m_Hint;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_TargetPositionWeight;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_TargetRotationWeight;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_HintWeight;

        [NotKeyable, SerializeField] bool m_MaintainTargetPositionOffset;
        [NotKeyable, SerializeField] bool m_MaintainTargetRotationOffset;

        public Transform root { get => m_Root; set => m_Root = value; }
        public Transform mid { get => m_Mid; set => m_Mid = value; }
        public Transform tip { get => m_Tip; set => m_Tip = value; }
        public Transform target { get => m_Target; set => m_Target = value; }
        public Transform hint { get => m_Hint; set => m_Hint = value; }

        public float targetPositionWeight { get => m_TargetPositionWeight; set => m_TargetPositionWeight = Mathf.Clamp01(value); }
        public float targetRotationWeight { get => m_TargetRotationWeight; set => m_TargetRotationWeight = Mathf.Clamp01(value); }
        public float hintWeight { get => m_HintWeight; set => m_HintWeight = Mathf.Clamp01(value); }

        public bool maintainTargetPositionOffset { get => m_MaintainTargetPositionOffset; set => m_MaintainTargetPositionOffset = value; }
        public bool maintainTargetRotationOffset { get => m_MaintainTargetRotationOffset; set => m_MaintainTargetRotationOffset = value; }

        string ITwoBoneIKConstraintData.targetPositionWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_TargetPositionWeight));
        string ITwoBoneIKConstraintData.targetRotationWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_TargetRotationWeight));
        string ITwoBoneIKConstraintData.hintWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_HintWeight));

        bool IAnimationJobData.IsValid() => !(m_Tip == null || m_Mid == null || m_Root == null || m_Target == null);

        void IAnimationJobData.SetDefaultValues()
        {
            m_Root = null;
            m_Mid = null;
            m_Tip = null;
            m_Target = null;
            m_Hint = null;
            m_TargetPositionWeight = 1f;
            m_TargetRotationWeight = 1f;
            m_HintWeight = 1f;
            m_MaintainTargetPositionOffset = false;
            m_MaintainTargetRotationOffset = false;
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Two Bone IK Constraint")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/TwoBoneIKConstraint.html")]
    public class TwoBoneIKConstraint : RigConstraint<
        TwoBoneIKConstraintJob,
        TwoBoneIKConstraintData,
        TwoBoneIKConstraintJobBinder<TwoBoneIKConstraintData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
        [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif
    }
}
