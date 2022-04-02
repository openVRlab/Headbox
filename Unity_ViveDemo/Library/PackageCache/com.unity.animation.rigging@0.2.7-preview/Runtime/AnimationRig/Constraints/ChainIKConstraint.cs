namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct ChainIKConstraintData : IAnimationJobData, IChainIKConstraintData
    {
        [SerializeField] Transform m_Root;
        [SerializeField] Transform m_Tip;

        [SyncSceneToStream, SerializeField] Transform m_Target;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_ChainRotationWeight;
        [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_TipRotationWeight;

        [NotKeyable, SerializeField, Range(1, 50)] int m_MaxIterations;
        [NotKeyable, SerializeField, Range(0f, 0.01f)] float m_Tolerance;
        [NotKeyable, SerializeField] bool m_MaintainTargetPositionOffset;
        [NotKeyable, SerializeField] bool m_MaintainTargetRotationOffset;

        public Transform root { get => m_Root; set => m_Root = value; }
        public Transform tip { get => m_Tip; set => m_Tip = value; }
        public Transform target { get => m_Target; set => m_Target = value; }
        public float chainRotationWeight { get => m_ChainRotationWeight; set => m_ChainRotationWeight = Mathf.Clamp01(value); }
        public float tipRotationWeight { get => m_TipRotationWeight; set => m_TipRotationWeight = Mathf.Clamp01(value); }
        public int maxIterations { get => m_MaxIterations; set => m_MaxIterations = Mathf.Clamp(value, 1, 50); }
        public float tolerance { get => m_Tolerance; set => m_Tolerance = Mathf.Clamp(value, 0f, 0.01f); }
        public bool maintainTargetPositionOffset { get => m_MaintainTargetPositionOffset; set => m_MaintainTargetPositionOffset = value; }
        public bool maintainTargetRotationOffset { get => m_MaintainTargetRotationOffset; set => m_MaintainTargetRotationOffset = value; }

        string IChainIKConstraintData.chainRotationWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_ChainRotationWeight));
        string IChainIKConstraintData.tipRotationWeightFloatProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_TipRotationWeight));

        bool IAnimationJobData.IsValid()
        {
            if (m_Root == null || m_Tip == null || m_Target == null)
                return false;

            int count = 1;
            Transform tmp = m_Tip;
            while (tmp != null && tmp != m_Root)
            {
                tmp = tmp.parent;
                ++count;
            }

            return (tmp == m_Root && count > 2);
        }

        void IAnimationJobData.SetDefaultValues()
        {
            m_Root = null;
            m_Tip = null;
            m_Target = null;
            m_ChainRotationWeight = 1f;
            m_TipRotationWeight = 1f;
            m_MaxIterations = 15;
            m_Tolerance = 0.0001f;
            m_MaintainTargetPositionOffset = false;
            m_MaintainTargetRotationOffset = false;
        }
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Chain IK Constraint")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/ChainIKConstraint.html")]
    public class ChainIKConstraint : RigConstraint<
        ChainIKConstraintJob,
        ChainIKConstraintData,
        ChainIKConstraintJobBinder<ChainIKConstraintData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
        [NotKeyable, SerializeField, HideInInspector] bool m_SettingsGUIToggle;
    #endif
    }
}
