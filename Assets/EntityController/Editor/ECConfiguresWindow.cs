using UnityEngine;
using UnityEditor;
using System.IO;

public class ECConfiguresWindow : EditorWindow {
    private const string CLASS = "ECConstants";
    private const string PATH = "Assets/EntityController/Scripts/" + CLASS + ".cs";
    private const string TITLE = "EC Configures";

    private const string DEFAULT_STATIC_BLOCK = "StaticBlock";
    private const string DEFAULT_DYNAMIC_BLOCK = "DynamicBlock";
    private const string DEFAULT_ENTITY = "Entity";
    private const float DEFAULT_GRAVITY = 10f;
    private const float DEFAULT_FALL_LIMIT = 12f;
    private const float DEFAULT_SLOOP_LIMIT = 50f;
    

    private string m_staticBlock = DEFAULT_STATIC_BLOCK;
    private string m_dynamicBlock = DEFAULT_DYNAMIC_BLOCK;
    private string m_entity = DEFAULT_ENTITY;

    private float m_gravity = 10f;
    private float m_fallLimit = 12f;
    private float m_slopeLimit = 50f;

    private bool m_foldCollision = true;
    private bool m_foldCharacter = true;
    
    [MenuItem("Stroy/" + TITLE)]
    private static void Init() {
        ECConfiguresWindow window = GetWindow(typeof(ECConfiguresWindow)) as ECConfiguresWindow;
        window.titleContent.text = TITLE;
        window.Show();
    }
    
    private void OnGUI() {

        m_foldCollision = EditorGUILayout.Foldout(m_foldCollision, "Collision");
        if (m_foldCollision) {
            m_staticBlock = EditorGUILayout.TextField(DEFAULT_STATIC_BLOCK, m_staticBlock);
            m_dynamicBlock = EditorGUILayout.TextField(DEFAULT_DYNAMIC_BLOCK, m_dynamicBlock);
            m_entity = EditorGUILayout.TextField(DEFAULT_ENTITY, m_entity);
        }

        GUILayout.Space(16);

        m_foldCharacter = EditorGUILayout.Foldout(m_foldCharacter, "Character");
        if (m_foldCharacter) {
            m_gravity = EditorGUILayout.FloatField("Gravity", m_gravity);
            m_fallLimit = EditorGUILayout.FloatField("Fall Limit", m_fallLimit);
            m_slopeLimit = EditorGUILayout.FloatField("Slope Limit", m_slopeLimit);
        }

        if (GUILayout.Button("Save")) {
            SaveValues();
        }
        if (GUILayout.Button("Default")) {
            SetDefaultValues();
        }
    }

    private void SaveValues() {
        string contents = 
            "namespace Stroy { \n" +
            "   namespace EC { \n" +
            "       public static class " + CLASS + " { \n" +
            $"           public const int StaticBlock = {LayerMask.NameToLayer(m_staticBlock)}; \n" +
            $"           public const int DynamicBlock = {LayerMask.NameToLayer(m_dynamicBlock)}; \n" +
            $"           public const int Entity = {LayerMask.NameToLayer(m_entity)}; \n" +
            "           public const int BlockMask = 0x01 << StaticBlock | 0x01 << DynamicBlock; \n" +
            "\n" +
            "           public const float MinContactOffset = 0.01f; \n" +
            "           public const float MaxContactOffset = 0.02f; \n" +
            "\n" +
            $"           public const float Gravity = {m_gravity}f; \n" +
            $"           public const float FallLimit = {m_fallLimit}f; \n" +
            $"           public const float SlopeLimit = {m_slopeLimit}f; \n" +
            "       } \n" +
            "   } \n" +
            "} \n";
        File.WriteAllText(PATH, contents);
    }

    private void SetDefaultValues() {
        m_staticBlock = DEFAULT_STATIC_BLOCK;
        m_dynamicBlock = DEFAULT_DYNAMIC_BLOCK;
        m_entity = DEFAULT_ENTITY;

        m_gravity = DEFAULT_GRAVITY;
        m_fallLimit = DEFAULT_FALL_LIMIT;
        m_slopeLimit = DEFAULT_SLOOP_LIMIT;
    }
}