using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(ChainIKConstraint))]
    public class ChainIKConstraintEditor : Editor
    {
        static readonly GUIContent k_SourceObjectLabel = new GUIContent("Source Object");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");
        static readonly GUIContent k_MaintainTargetOffsetLabel = new GUIContent("Maintain Target Offset");

        SerializedProperty m_Weight;
        SerializedProperty m_Root;
        SerializedProperty m_Tip;
        SerializedProperty m_Target;
        SerializedProperty m_ChainRotationWeight;
        SerializedProperty m_TipRotationWeight;
        SerializedProperty m_MaxIterations;
        SerializedProperty m_Tolerance;
        SerializedProperty m_MaintainTargetPositionOffset;
        SerializedProperty m_MaintainTargetRotationOffset;

        SerializedProperty m_SourceObjectsToggle;
        SerializedProperty m_SettingsToggle;

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");
            m_SourceObjectsToggle = serializedObject.FindProperty("m_SourceObjectsGUIToggle");
            m_SettingsToggle = serializedObject.FindProperty("m_SettingsGUIToggle");

            var data = serializedObject.FindProperty("m_Data");
            m_Root = data.FindPropertyRelative("m_Root");
            m_Tip = data.FindPropertyRelative("m_Tip");
            m_Target = data.FindPropertyRelative("m_Target");
            m_ChainRotationWeight = data.FindPropertyRelative("m_ChainRotationWeight");
            m_TipRotationWeight = data.FindPropertyRelative("m_TipRotationWeight");
            m_MaxIterations = data.FindPropertyRelative("m_MaxIterations");
            m_Tolerance = data.FindPropertyRelative("m_Tolerance");
            m_MaintainTargetPositionOffset = data.FindPropertyRelative("m_MaintainTargetPositionOffset");
            m_MaintainTargetRotationOffset = data.FindPropertyRelative("m_MaintainTargetRotationOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight);
            EditorGUILayout.PropertyField(m_Root);
            EditorGUILayout.PropertyField(m_Tip);

            m_SourceObjectsToggle.boolValue = EditorGUILayout.Foldout(m_SourceObjectsToggle.boolValue, k_SourceObjectLabel);
            if (m_SourceObjectsToggle.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Target);
                EditorGUI.indentLevel--;
            }

            m_SettingsToggle.boolValue = EditorGUILayout.Foldout(m_SettingsToggle.boolValue, k_SettingsLabel);
            if (m_SettingsToggle.boolValue)
            {
                EditorGUI.indentLevel++;
                MaintainOffsetHelper.DoDropdown(k_MaintainTargetOffsetLabel, m_MaintainTargetPositionOffset, m_MaintainTargetRotationOffset);
                EditorGUILayout.PropertyField(m_ChainRotationWeight);
                EditorGUILayout.PropertyField(m_TipRotationWeight);
                EditorGUILayout.PropertyField(m_MaxIterations);
                EditorGUILayout.PropertyField(m_Tolerance);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
