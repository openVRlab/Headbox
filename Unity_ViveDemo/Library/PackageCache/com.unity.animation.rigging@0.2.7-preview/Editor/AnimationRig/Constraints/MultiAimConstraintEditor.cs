using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditorInternal;
using System.Reflection;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(MultiAimConstraint))]
    public class MultiAimConstraintEditor : Editor
    {
        static readonly GUIContent k_SourceObjectsLabel = new GUIContent("Source Objects");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");
        static readonly GUIContent k_AimAxisLabel = new GUIContent("Aim Axis");
        static readonly string[] k_AimAxisLabels = { "X", "-X", "Y", "-Y", "Z", "-Z" };
        static readonly GUIContent k_MaintainOffsetLabel = new GUIContent("Maintain Rotation Offset");

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_AimAxis;
        SerializedProperty m_SourceObjects;
        SerializedProperty m_MaintainOffset;
        SerializedProperty m_Offset;
        SerializedProperty m_ConstrainedAxes;
        SerializedProperty m_MinLimit;
        SerializedProperty m_MaxLimit;

        SerializedProperty m_SourceObjectsToggle;
        SerializedProperty m_SettingsToggle;
        ReorderableList m_ReorderableList;
        MultiAimConstraint m_Constraint;
        WeightedTransformArray m_SourceObjectsArray;

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");
            m_SourceObjectsToggle = serializedObject.FindProperty("m_SourceObjectsGUIToggle");
            m_SettingsToggle = serializedObject.FindProperty("m_SettingsGUIToggle");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_AimAxis = data.FindPropertyRelative("m_AimAxis");
            m_SourceObjects = data.FindPropertyRelative("m_SourceObjects");
            m_MaintainOffset = data.FindPropertyRelative("m_MaintainOffset");
            m_Offset = data.FindPropertyRelative("m_Offset");
            m_ConstrainedAxes = data.FindPropertyRelative("m_ConstrainedAxes");
            m_MinLimit = data.FindPropertyRelative("m_MinLimit");
            m_MaxLimit = data.FindPropertyRelative("m_MaxLimit");

            m_Constraint = (MultiAimConstraint)serializedObject.targetObject;
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

            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, k_AimAxisLabel, m_AimAxis);
            m_AimAxis.enumValueIndex = EditorGUI.Popup(rect, m_AimAxis.displayName, m_AimAxis.enumValueIndex, k_AimAxisLabels);
            EditorGUI.EndProperty();

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
                EditorGUILayout.PropertyField(m_MaintainOffset, k_MaintainOffsetLabel);
                EditorGUILayout.PropertyField(m_Offset);
                EditorGUILayout.PropertyField(m_ConstrainedAxes);
                EditorGUILayout.PropertyField(m_MinLimit);
                EditorGUILayout.PropertyField(m_MaxLimit);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
