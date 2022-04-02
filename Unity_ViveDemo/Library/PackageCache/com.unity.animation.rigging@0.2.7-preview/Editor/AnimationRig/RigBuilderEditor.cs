using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditorInternal;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(RigBuilder))]
    public class RigBuilderEditor : Editor
    {
        static readonly GUIContent k_RigLabel = new GUIContent("Rig Layers");

        SerializedProperty m_Rigs;
        ReorderableList m_ReorderableList;

        void OnEnable()
        {
            m_Rigs = serializedObject.FindProperty("m_RigLayers");
            m_ReorderableList = ReorderableListHelper.Create(serializedObject, m_Rigs, true, true);
            if (m_ReorderableList.count == 0)
                ((RigBuilder)serializedObject.targetObject).layers.Add(new RigBuilder.RigLayer(null));

            m_ReorderableList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, k_RigLabel);

            m_ReorderableList.onAddCallback = (ReorderableList list) =>
            {
                ((RigBuilder)(serializedObject.targetObject)).layers.Add(new RigBuilder.RigLayer(null, true));
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Separator();
            m_ReorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(RigBuilder.RigLayer))]
    public class RigLayerDrawer : PropertyDrawer
    {
        const int k_Padding = 6;
        const int k_TogglePadding = 30;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            var w = rect.width - k_TogglePadding;
            var weightRect = new Rect(rect.x + w + k_Padding, rect.y, rect.width - w - k_Padding, rect.height);
            rect.width = w;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("rig"), label);

            var indentLvl = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("active"), GUIContent.none);
            EditorGUI.indentLevel = indentLvl;

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }
    }
}
