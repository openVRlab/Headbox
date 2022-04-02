using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditorInternal;
using System.Reflection;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(MultiParentConstraint))]
    public class MultiParentConstraintEditor : Editor
    {
        static readonly GUIContent k_SourceObjectsLabel = new GUIContent("Source Objects");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");
        static readonly GUIContent k_MaintainOffsetLabel = new GUIContent("Maintain Offset");

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_ConstrainedPositionAxes;
        SerializedProperty m_ConstrainedRotationAxes;
        SerializedProperty m_SourceObjects;
        SerializedProperty m_MaintainPositionOffset;
        SerializedProperty m_MaintainRotationOffset;

        SerializedProperty m_SourceObjectsToggle;
        SerializedProperty m_SettingsToggle;
        ReorderableList m_ReorderableList;
        MultiParentConstraint m_Constraint;
        WeightedTransformArray m_SourceObjectsArray;

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");
            m_SourceObjectsToggle = serializedObject.FindProperty("m_SourceObjectsGUIToggle");
            m_SettingsToggle = serializedObject.FindProperty("m_SettingsGUIToggle");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_ConstrainedPositionAxes = data.FindPropertyRelative("m_ConstrainedPositionAxes");
            m_ConstrainedRotationAxes = data.FindPropertyRelative("m_ConstrainedRotationAxes");
            m_SourceObjects = data.FindPropertyRelative("m_SourceObjects");
            m_MaintainPositionOffset = data.FindPropertyRelative("m_MaintainPositionOffset");
            m_MaintainRotationOffset = data.FindPropertyRelative("m_MaintainRotationOffset");

            m_Constraint = (MultiParentConstraint)serializedObject.targetObject;
            m_SourceObjectsArray = m_Constraint.data.sourceObjects;

            var dataType = m_Constraint.data.GetType();
            var fieldInfo = dataType.GetField("m_SourceObjects", BindingFlags.NonPublic | BindingFlags.Instance);
            var range = fieldInfo.GetCustomAttribute<RangeAttribute>();

            if (m_SourceObjectsArray.Count == 0)
            {
                m_SourceObjectsArray.Add(WeightedTransform.Default(1f));
                m_Constraint.data.sourceObjects = m_SourceObjectsArray;
            }

            m_ReorderableList = WeightedTransformHelper.CreateReorderableList(m_SourceObjects, ref m_SourceObjectsArray, range);

            m_ReorderableList.onChangedCallback = (ReorderableList reorderableList) =>
            {
                m_Constraint.data.sourceObjects = (WeightedTransformArray)reorderableList.list;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight);
            EditorGUILayout.PropertyField(m_ConstrainedObject);

            m_SourceObjectsToggle.boolValue = EditorGUILayout.Foldout(m_SourceObjectsToggle.boolValue, k_SourceObjectsLabel);
            if (m_SourceObjectsToggle.boolValue)
            {
                // Sync list with sourceObjects.
                m_ReorderableList.list = m_Constraint.data.sourceObjects;

                EditorGUI.indentLevel++;
                m_ReorderableList.DoLayoutList();
                EditorGUI.indentLevel--;
            }

            m_SettingsToggle.boolValue = EditorGUILayout.Foldout(m_SettingsToggle.boolValue, k_SettingsLabel);
            if (m_SettingsToggle.boolValue)
            {
                EditorGUI.indentLevel++;
                MaintainOffsetHelper.DoDropdown(k_MaintainOffsetLabel, m_MaintainPositionOffset, m_MaintainRotationOffset);
                EditorGUILayout.PropertyField(m_ConstrainedPositionAxes);
                EditorGUILayout.PropertyField(m_ConstrainedRotationAxes);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
