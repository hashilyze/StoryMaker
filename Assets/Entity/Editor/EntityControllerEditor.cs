using UnityEngine;
using UnityEditor;


namespace Stroy.Entity {
    [CustomEditor(typeof(EntityController))]
    [CanEditMultipleObjects]
    public class EntityControllerEditor : Editor {
        private SerializedProperty m_velocity;
        private SerializedProperty m_size;
        private SerializedProperty m_followPlatform;
        private SerializedProperty m_useGravity;
        private SerializedProperty m_gravityScale;

        private void OnEnable() {
            m_velocity = serializedObject.FindProperty("m_velocity");
            m_size = serializedObject.FindProperty("m_size");
            m_followPlatform = serializedObject.FindProperty("m_followPlatform");
            m_useGravity = serializedObject.FindProperty("m_useGravity");
            m_gravityScale = serializedObject.FindProperty("m_gravityScale");
        }

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_velocity);
            EditorGUILayout.PropertyField(m_size);
            EditorGUILayout.PropertyField(m_followPlatform);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(m_useGravity);
            EditorGUILayout.PropertyField(m_gravityScale);
            serializedObject.ApplyModifiedProperties();
        }
    }
}