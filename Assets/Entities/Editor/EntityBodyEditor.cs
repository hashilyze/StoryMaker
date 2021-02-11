using UnityEngine;
using UnityEditor;


namespace Stroy.Entities {
    [CustomEditor(typeof(EntityBody))]
    [CanEditMultipleObjects]
    public class EntityBodyEditor : Editor {
        private static bool m_toggleInfo = true;

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

            EditorGUILayout.PropertyField(m_useGravity);
            if (m_useGravity.boolValue) {
                EditorGUILayout.PropertyField(m_gravityScale);
            }

            m_toggleInfo = EditorGUILayout.BeginFoldoutHeaderGroup(m_toggleInfo, "Info");
            if (m_toggleInfo) {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_velocity);
                EditorGUILayout.PropertyField(m_size);
                EditorGUILayout.PropertyField(m_followPlatform);
                GUI.enabled = true;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}