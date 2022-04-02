using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(OverrideTransform))]
    public class OverrideTransformEditor : Editor
    {
        static readonly GUIContent k_SourceObjectsLabel = new GUIContent("Source Objects");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_OverrideSource;
        SerializedProperty m_OverridePosition;
        SerializedProperty m_OverrideRotation;
        SerializedProperty m_Space;
        SerializedProperty m_PositionWeight;
        SerializedProperty m_RotationWeight;

        SerializedProperty m_SourceObjectsToggle;
        SerializedProperty m_SettingsToggle;

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");
            m_SourceObjectsToggle = serializedObject.FindProperty("m_SourceObjectsGUIToggle");
            m_SettingsToggle = serializedObject.FindProperty("m_SettingsGUIToggle");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_OverrideSource = data.FindPropertyRelative("m_OverrideSource");
            m_OverridePosition = data.FindPropertyRelative("m_OverridePosition");
            m_OverrideRotation = data.FindPropertyRelative("m_OverrideRotation");
            m_Space = data.FindPropertyRelative("m_Space");
            m_PositionWeight = data.FindPropertyRelative("m_PositionWeight");
            m_RotationWeight = data.FindPropertyRelative("m_RotationWeight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight);
            EditorGUILayout.PropertyField(m_ConstrainedObject);

            m_SourceObjectsToggle.boolValue = EditorGUILayout.Foldout(m_SourceObjectsToggle.boolValue, k_SourceObjectsLabel);
            if (m_SourceObjectsToggle.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_OverrideSource);
                using (new EditorGUI.DisabledScope(m_OverrideSource.objectReferenceValue != null))
                {
                    EditorGUILayout.PropertyField(m_OverridePosition);
                    EditorGUILayout.PropertyField(m_OverrideRotation);
                }
                EditorGUI.indentLevel--;
            }

            m_SettingsToggle.boolValue = EditorGUILayout.Foldout(m_SettingsToggle.boolValue, k_SettingsLabel);
            if (m_SettingsToggle.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Space);
                EditorGUILayout.PropertyField(m_PositionWeight);
                EditorGUILayout.PropertyField(m_RotationWeight);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
