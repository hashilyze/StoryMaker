using UnityEngine;
using UnityEditor;

namespace Stroy.Entities {
    [CustomEditor(typeof(Character))]
    [CanEditMultipleObjects]
    public class CharacterEditor : Editor {
        private static bool toggleGroundContact = true;
        private static bool toggleWallContact = true;
        private static bool toggleCellContact = true;

        private SerializedProperty m_entityState;
        private SerializedProperty m_velocity;

        private SerializedProperty m_isGround;
        private SerializedProperty m_contactRadian;
        private SerializedProperty m_contactGround;

        private SerializedProperty m_activeWallSensor;
        private SerializedProperty m_isWall;
        private SerializedProperty m_contactWall;
        private SerializedProperty m_wallOnLeft;
        private SerializedProperty m_wallOnRight;

        private SerializedProperty m_isCell;


        private void OnEnable() {
            m_entityState = serializedObject.FindProperty("State");
            m_velocity = serializedObject.FindProperty("m_velocity");

            m_isGround = serializedObject.FindProperty("m_isGround");
            m_contactRadian = serializedObject.FindProperty("m_contactRadian");
            m_contactGround = serializedObject.FindProperty("m_contactGround");

            m_activeWallSensor = serializedObject.FindProperty("m_activeWallSensor");
            m_isWall = serializedObject.FindProperty("m_isWall");
            m_contactWall = serializedObject.FindProperty("m_contactWall");
            m_wallOnLeft = serializedObject.FindProperty("m_wallOnLeft");
            m_wallOnRight = serializedObject.FindProperty("m_wallOnRight");

            m_isCell = serializedObject.FindProperty("m_isCell");
        }

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_entityState);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_velocity);
            GUI.enabled = true;

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

            // Cell Contact
            toggleCellContact = EditorGUILayout.BeginFoldoutHeaderGroup(toggleCellContact, "Contact Cell");
            if (toggleCellContact) {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_isCell);
                GUI.enabled = true;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}