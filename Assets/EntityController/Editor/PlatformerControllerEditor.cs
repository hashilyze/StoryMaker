using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Stroy.EC;

[CustomEditor(typeof(PlatformerController))]
[CanEditMultipleObjects]
public class PlatformerControllerEditor : Editor {
    private static bool toggleGroundContact = true;
    private static bool toggleWallContact = true;

    private SerializedProperty m_velocity;
    private SerializedProperty m_collision;
    private SerializedProperty m_gravityScale;

    private SerializedProperty m_isGround;
    private SerializedProperty m_contactRadian;
    private SerializedProperty m_contactGround;

    private SerializedProperty m_activeWallCheck;
    private SerializedProperty m_isWall;
    private SerializedProperty m_contactWall;

    private void OnEnable() {
        m_velocity = serializedObject.FindProperty("m_velocity");
        m_collision = serializedObject.FindProperty("m_collision");
        m_gravityScale = serializedObject.FindProperty("m_gravityScale");

        m_isGround = serializedObject.FindProperty("m_isGround");
        m_contactRadian = serializedObject.FindProperty("m_contactRadian");
        m_contactGround = serializedObject.FindProperty("m_contactGround");

        m_activeWallCheck = serializedObject.FindProperty("m_activeWallCheck");
        m_isWall = serializedObject.FindProperty("m_isWall");
        m_contactWall = serializedObject.FindProperty("m_contactWall");
    }

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();

        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(m_velocity);
        EditorGUILayout.PropertyField(m_collision);
        EditorGUILayout.PropertyField(m_gravityScale);
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
        EditorGUILayout.PropertyField(m_activeWallCheck);
        if (m_activeWallCheck.boolValue) {
            toggleWallContact = EditorGUILayout.BeginFoldoutHeaderGroup(toggleWallContact, "Contact Wall");
            if (toggleWallContact) {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_isWall);
                EditorGUILayout.PropertyField(m_contactWall);

                string wallInfo = string.Empty;
                if (m_collision.hasMultipleDifferentValues) {
                    wallInfo = "-";
                } else {
                    ECollision collision = (target as PlatformerController).Collision;

                    if (collision.HasFlag(ECollision.Left)) wallInfo += "Left, ";
                    if (collision.HasFlag(ECollision.Right)) wallInfo += "Right, ";
                    if (wallInfo == string.Empty) wallInfo = "No Wall";
                }
                EditorGUILayout.LabelField("Wall", wallInfo);

                GUI.enabled = true;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
