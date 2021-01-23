﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stroy.Entity.EntityController))]
[CanEditMultipleObjects]
public class EntityControllerEditor : Editor {
    private SerializedProperty m_velocity;
    private SerializedProperty m_size;
    private SerializedProperty m_follower;

    private void OnEnable() {
        m_velocity = serializedObject.FindProperty("m_velocity");
        m_size = serializedObject.FindProperty("m_size");
        m_follower = serializedObject.FindProperty("m_follower");
    }

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        serializedObject.Update();
        GUI.enabled = false;
        EditorGUILayout.PropertyField(m_velocity);
        EditorGUILayout.PropertyField(m_size);
        EditorGUILayout.PropertyField(m_follower);
        GUI.enabled = true;
        serializedObject.ApplyModifiedProperties();
    }
}