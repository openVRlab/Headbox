using System;

namespace UnityEngine.Animations.Rigging
{
    [Serializable]
    public class RigEffectorData
    {
        [Serializable]
        public struct Style
        {
            public Mesh shape;
            public Color color;
            public float size;
            public Vector3 position;
            public Vector3 rotation;
        };

        [SerializeField] private Transform m_Transform;
        [SerializeField] private Style m_Style = new Style();
        [SerializeField] private bool m_Visible = true;

        public static Style defaultStyle
        {
            get
            {
                var style = new Style()
                {
                    shape =  Resources.Load<Mesh>("Shapes/LocatorEffector"),
                    color = new Color(1f, 0f, 0f, 0.5f),
                    size = 0.10f,
                    position = Vector3.zero,
                    rotation = Vector3.zero
                };

                return style;
            }
        }

        public Transform transform { get => m_Transform; }
        public Style style { get => m_Style; }
        public bool visible { get => m_Visible; set => m_Visible = value; }

        public void Initialize(Transform transform, Style style)
        {
            m_Transform = transform;
            m_Style = style;
        }
    }
}
