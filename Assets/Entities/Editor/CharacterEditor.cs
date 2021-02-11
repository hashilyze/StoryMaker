using UnityEngine;
using UnityEditor;

namespace Stroy.Entities {
    [CustomEditor(typeof(Character))]
    [CanEditMultipleObjects]
    public class CharacterEditor : Editor {
        private static bool toggleGroundContact = true;
        private static bool toggleWallContact = true;
        private static bool toggleCeilContact = true;

        private SerializedProperty m_velocity;
        private SerializedProperty m_face;
        private SerializedProperty m_isSurfaceSnap;

        private SerializedProperty m_isGround;
        private SerializedProperty m_contactRadian;
        private SerializedProperty m_contactGround;

        private SerializedProperty m_activeWallSensor;
        private SerializedProperty m_isWall;
        private SerializedProperty m_contactWall;
        private SerializedProperty m_wallOnLeft;
        private SerializedProperty m_wallOnRight;

        private SerializedProperty m_isCeil;


        private void OnEnable() {
            m_velocity = serializedObject.FindProperty("m_velocity");
            m_face = serializedObject.FindProperty("m_face");
            m_isSurfaceSnap = serializedObject.FindProperty("m_isSurfaceSnap");

            m_isGround = serializedObject.FindProperty("m_isGround");
            m_contactRadian = serializedObject.FindProperty("m_contactRadian");
            m_contactGround = serializedObject.FindProperty("m_contactGround");

            m_activeWallSensor = serializedObject.FindProperty("m_activeWallSensor");
            m_isWall = serializedObject.FindProperty("m_isWall");
            m_contactWall = serializedObject.FindProperty("m_contactWall");
            m_wallOnLeft = serializedObject.FindProperty("m_wallOnLeft");
            m_wallOnRight = serializedObject.FindProperty("m_wallOnRight");

            m_isCeil = serializedObject.FindProperty("m_isCeil");
        }

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_velocity);
            EditorGUILayout.PropertyField(m_face);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(m_isSurfaceSnap);

            // Ground Contact
            toggleGroundContact = EditorGUILayout.BeginFoldoutHeaderGroup(toggleGroundContact, "Contact Ground");
            if (toggleGroundContact) {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_isGround);
                EditorGUILayout.PropertyField(m_contactRadian);
                EditorGUILayout.PropertyField(m_contactGround);
                GUI.enabled = true;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            // Wall Contact
            EditorGUILayout.PropertyField(m_activeWallSensor);
            if (m_activeWallSensor.boolValue) {
                toggleWallContact = EditorGUILayout.BeginFoldoutHeaderGroup(toggleWallContact, "Contact Wall");
                if (toggleWallContact) {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(m_isWall);
                    EditorGUILayout.PropertyField(m_contactWall);
                    EditorGUILayout.PropertyField(m_wallOnLeft);
                    EditorGUILayout.PropertyField(m_wallOnRight);
                    GUI.enabled = true;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Ceil Contact
            toggleCeilContact = EditorGUILayout.BeginFoldoutHeaderGroup(toggleCeilContact, "Contact Ceil");
            if (toggleCeilContact) {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_isCeil);
                GUI.enabled = true;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}