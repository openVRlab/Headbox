using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditorInternal;
using System.Reflection;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(TwistCorrection))]
    public class TwistCorrectionEditor : Editor
    {
        static readonly GUIContent k_TwistNodesLabel = new GUIContent("Twist Nodes");

        SerializedProperty m_Weight;
        SerializedProperty m_Source;
        SerializedProperty m_TwistAxis;
        SerializedProperty m_TwistNodes;

        SerializedProperty m_TwistNodesToggle;
        ReorderableList m_ReorderableList;
        TwistCorrection m_Constraint;
        WeightedTransformArray m_TwistNodesArray;

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");
            m_TwistNodesToggle = serializedObject.FindProperty("m_TwistNodesGUIToggle");

            var data = serializedObject.FindProperty("m_Data");
            m_Source = data.FindPropertyRelative("m_Source");
            m_TwistAxis = data.FindPropertyRelative("m_TwistAxis");
            m_TwistNodes = data.FindPropertyRelative("m_TwistNodes");

            m_Constraint = (TwistCorrection)serializedObject.targetObject;
            m_TwistNodesArray = m_Constraint.data.twistNodes;

            var dataType = m_Constraint.data.GetType();
            var fieldInfo = dataType.GetField("m_TwistNodes", BindingFlags.NonPublic | BindingFlags.Instance);
            var range = fieldInfo.GetCustomAttribute<RangeAttribute>();

            if (m_TwistNodesArray.Count == 0)
            {
                m_TwistNodesArray.Add(WeightedTransform.Default(0f));
                m_Constraint.data.twistNodes = m_TwistNodesArray;
            }

            m_ReorderableList = WeightedTransformHelper.CreateReorderableList(m_TwistNodes, ref m_TwistNodesArray, range);

            m_ReorderableList.onChangedCallback = (ReorderableList reorderableList) =>
            {
                m_Constraint.data.twistNodes = (WeightedTransformArray)reorderableList.list;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight);
            EditorGUILayout.PropertyField(m_Source);
            EditorGUILayout.PropertyField(m_TwistAxis);

            m_TwistNodesToggle.boolValue = EditorGUILayout.Foldout(m_TwistNodesToggle.boolValue, k_TwistNodesLabel);
            if (m_TwistNodesToggle.boolValue)
            {
                EditorGUI.indentLevel++;
                m_ReorderableList.DoLayoutList();
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
