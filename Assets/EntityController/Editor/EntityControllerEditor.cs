using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stroy.EC.EntityController))]
[CanEditMultipleObjects]
public class EntityControllerEditor : Editor {
    private SerializedProperty m_velocity;
    private SerializedProperty m_size;
    private SerializedProperty m_followBlock;

    private void OnEnable() {
        m_velocity = serializedObject.FindProperty("m_velocity");
        m_size = serializedObject.FindProperty("m_size");
        m_followBlock = serializedObject.FindProperty("m_followBlock");
    }

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        serializedObject.Update();
        GUI.enabled = false;
        EditorGUILayout.PropertyField(m_velocity);
        EditorGUILayout.PropertyField(m_size);
        EditorGUILayout.PropertyField(m_followBlock);
        GUI.enabled = true;
        serializedObject.ApplyModifiedProperties();
    }
}
