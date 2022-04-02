using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(DampedTransform))]
    public class DampedTransformEditor : Editor
    {
        static readonly GUIContent k_SourceObjectLabel = new GUIContent("Source Object");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_Source;
        SerializedProperty m_DampPosition;
        SerializedProperty m_DampRotation;
        SerializedProperty m_MaintainAim;

        SerializedProperty m_SourceObjectsToggle;
        SerializedProperty m_SettingsToggle;
    
        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");
            m_SourceObjectsToggle = serializedObject.FindProperty("m_SourceObjectsGUIToggle");
            m_SettingsToggle = serializedObject.FindProperty("m_SettingsGUIToggle");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_Source = data.FindPropertyRelative("m_Source");
            m_DampPosition = data.FindPropertyRelative("m_DampPosition");
            m_DampRotation = data.FindPropertyRelative("m_DampRotation");
            m_MaintainAim = data.FindPropertyRelative("m_MaintainAim");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight);
            EditorGUILayout.PropertyField(m_ConstrainedObject);

            m_SourceObjectsToggle.boolValue = EditorGUILayout.Foldout(m_SourceObjectsToggle.boolValue, k_SourceObjectLabel);
            if (m_SourceObjectsToggle.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Source);
                EditorGUI.indentLevel--;
            }

            m_SettingsToggle.boolValue = EditorGUILayout.Foldout(m_SettingsToggle.boolValue, k_SettingsLabel);
            if (m_SettingsToggle.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_DampPosition);
                EditorGUILayout.PropertyField(m_DampRotation);
                EditorGUILayout.PropertyField(m_MaintainAim);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}