using System.Collections.Generic;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Setup/Rig")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/index.html")]
    public class Rig : MonoBehaviour, IRigEffectorHolder
    {
        [SerializeField, Range(0f, 1f)]
        protected float m_Weight = 1f;

        private IRigConstraint[] m_Constraints;
        private IAnimationJob[]  m_Jobs;

#if UNITY_EDITOR
        [SerializeField] private List<RigEffectorData> m_Effectors = new List<RigEffectorData>();
        public IEnumerable<RigEffectorData> effectors { get => m_Effectors; }
#endif

        public bool Initialize(Animator animator)
        {
            if (isInitialized)
                return true;

            m_Constraints = RigUtils.GetConstraints(this);
            if (m_Constraints == null)
                return false;

            m_Jobs = RigUtils.CreateAnimationJobs(animator, m_Constraints);

            return (isInitialized = true);
        }

        public void Destroy()
        {
            if (!isInitialized)
                return;

            RigUtils.DestroyAnimationJobs(m_Constraints, m_Jobs);
            m_Constraints = null;
            m_Jobs = null;

            isInitialized = false;
        }

        public void UpdateConstraints()
        {
            if (!isInitialized)
                return;

            for (int i = 0, count = m_Constraints.Length; i < count; ++i)
                m_Constraints[i].UpdateJob(m_Jobs[i]);
        }

#if UNITY_EDITOR
        public void AddEffector(Transform transform)
        {
            var effector = new RigEffectorData();
            effector.Initialize(transform, RigEffectorData.defaultStyle);

            m_Effectors.Add(effector);
        }

        public void RemoveEffector(Transform transform)
        {
            m_Effectors.RemoveAll((RigEffectorData data) => data.transform == transform);
        }

        public bool ContainsEffector(Transform transform)
        {
            return m_Effectors.Exists((RigEffectorData data) => data.transform == transform);
        }
#endif

        public bool isInitialized { get; private set; }

        public float weight { get => m_Weight; set => m_Weight = Mathf.Clamp01(value); }

        public IRigConstraint[] constraints => isInitialized ? m_Constraints : null;

        public IAnimationJob[] jobs => isInitialized ? m_Jobs : null;
    }
}
