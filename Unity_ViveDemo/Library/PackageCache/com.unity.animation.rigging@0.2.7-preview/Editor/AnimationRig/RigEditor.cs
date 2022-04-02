using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(Rig))]
    public class RigEditor : Editor
    {
        SerializedProperty m_Weight;

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(m_Weight);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
