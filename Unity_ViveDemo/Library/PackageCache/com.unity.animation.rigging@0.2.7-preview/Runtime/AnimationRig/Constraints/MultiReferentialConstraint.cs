using System.Collections.Generic;

namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct MultiReferentialConstraintData : IAnimationJobData, IMultiReferentialConstraintData
    {
        [SyncSceneToStream, SerializeField] int m_Driver;
        [SyncSceneToStream, SerializeField] List<Transform> m_SourceObjects;

        public int driver
        {
            get => m_Driver;
            set => m_Driver = Mathf.Clamp(value, 0, m_SourceObjects.Count - 1);
        }

        public List<Transform> sourceObjects
        {
            get
            {
                if (m_SourceObjects == null)
                    m_SourceObjects = new List<Transform>();

                return m_SourceObjects;
            }

            set
            {
                m_SourceObjects = value;
                m_Driver = Mathf.Clamp(m_Driver, 0, m_SourceObjects.Count - 1);
            }
        }

        Transform[] IMultiReferentialConstraintData.sourceObjects => m_SourceObjects.ToArray();
        int IMultiReferentialConstraintData.driverValue => m_Driver;
        string IMultiReferentialConstraintData.driverIntProperty => PropertyUtils.ConstructConstraintDataPropertyName(nameof(m_Driver));

        bool IAnimationJobData.IsValid()
        {
            if (m_SourceObjects.Count < 2)
                return false;

            foreach (var src in m_SourceObjects)
                if (src.transform == null)
                    return false;

            return true;
        }

        void IAnimationJobData.SetDefaultValues()
        {
            m_Driver = 0;
            m_SourceObjects = new List<Transform>();
        }

        public void UpdateDriver() =>
            m_Driver = Mathf.Clamp(m_Driver, 0, m_SourceObjects != null ? m_SourceObjects.Count - 1 : 0);
    }

    [DisallowMultipleComponent, AddComponentMenu("Animation Rigging/Multi-Referential Constraint")]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@0.2?preview=1&subfolder=/manual/constraints/MultiReferentialConstraint.html")]
    public class MultiReferentialConstraint : RigConstraint<
        MultiReferentialConstraintJob,
        MultiReferentialConstraintData,
        MultiReferentialConstraintJobBinder<MultiReferentialConstraintData>
        >
    {
    #if UNITY_EDITOR
    #pragma warning disable 0414
        [NotKeyable, SerializeField, HideInInspector] bool m_SourceObjectsGUIToggle;
    #endif

        private void OnValidate()
        {
            m_Data.UpdateDriver();
        }
    }
}
