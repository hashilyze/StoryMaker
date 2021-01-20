using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public class ECConfiguresWindow : EditorWindow {
    private const string ROOT_PATH = "Assets/EntityController";
    private const string SCRIPT_PATH = ROOT_PATH + "/Scripts/" + SCRIPT + ".cs";
    private const string CONFIG_PATH = ROOT_PATH + "/" + CONFIG + ".json";

    private const string SCRIPT = "ECConstants";
    private const string CONFIG = "ECConfig";

    private const string TITLE = "EC Configures";

    public const string DefaultStaticBlock = "StaticBlock";
    public const string DefaultDynamicBlock = "DynamicBlock";
    public const string DefaultEntity = "Entity";
    public const float DefaultMinContactOffset = 0.01f;
    public const float DefaultMaxContactOffset = 0.02f;
    public const float DefaultGravity = 10f;
    public const float DefaultFallLimit = 12f;
    public const float DefaultSlopeLimit = 50f;
    public const float DefaultWallLimit = 0.1f;
    public const float DefaultDefaultGravityScale = 1f;
    public const float DefaultFallGravityScale = 1.5f;



    public string StaticBlock;
    public string DynamicBlock;
    public string Entity;
    public float MinContactOffset;
    public float MaxContactOffset;
    public float Gravity;
    public float FallLimit;
    public float SlopeLimit;
    public float WallLimit;
    public float DefaultGravityScale;
    public float FallGravityScale;


    private bool m_foldCollision = true;
    private bool m_foldCharacter = true;

    private string script;
    private int group;



    [MenuItem("Stroy/" + TITLE)]
    private static void Init() {
        ECConfiguresWindow window = GetWindow(typeof(ECConfiguresWindow)) as ECConfiguresWindow;
        window.titleContent.text = TITLE;
        window.Load();
        window.Show();
    }

    private void OnGUI() {
        m_foldCollision = EditorGUILayout.Foldout(m_foldCollision, "Collision");
        if (m_foldCollision) {
            StaticBlock = EditorGUILayout.TextField("StaticBlock", StaticBlock);
            DynamicBlock = EditorGUILayout.TextField("DynamicBlock", DynamicBlock);
            Entity = EditorGUILayout.TextField("Entity", Entity);
            MinContactOffset = EditorGUILayout.FloatField("MinContactOffset", MinContactOffset);
            MaxContactOffset = EditorGUILayout.FloatField("MaxContactOffset", MaxContactOffset);
        }
        GUILayout.Space(16);

        m_foldCharacter = EditorGUILayout.Foldout(m_foldCharacter, "Platform");
        if (m_foldCharacter) {
            Gravity = EditorGUILayout.FloatField("Gravity", Gravity);
            FallLimit = EditorGUILayout.FloatField("Fall Limit", FallLimit);
            SlopeLimit = EditorGUILayout.FloatField("Slope Limit", SlopeLimit);
            WallLimit = EditorGUILayout.FloatField("Wall Limit", WallLimit);
            DefaultGravityScale = EditorGUILayout.FloatField("DefaultGravityScale", DefaultGravityScale);
            FallGravityScale = EditorGUILayout.FloatField("FallGravityScale", FallGravityScale);
        }
        GUILayout.Space(16);

        if (GUILayout.Button("Save")) SaveValues();
        if (GUILayout.Button("Default")) SetDefaultValues();
    }

    private void SaveValues() {
        BeginScript();
        BeginNamespace("Stroy");
        BeginNamespace("EC");
        BeginClass(SCRIPT);
        AddProperty("int", "StaticBlock", LayerMask.NameToLayer(StaticBlock).ToString());
        AddProperty("int", "DynamicBlock", LayerMask.NameToLayer(DynamicBlock).ToString());
        AddProperty("int", "Entity", LayerMask.NameToLayer(Entity).ToString());
        AddProperty("int", "BlockMask", (0x01 << LayerMask.NameToLayer(StaticBlock) | 0x01 << LayerMask.NameToLayer(DynamicBlock)).ToString());
        AddSpace();
        AddProperty("float", "MinContactOffset", MinContactOffset + "f");
        AddProperty("float", "MaxContactOffset", MaxContactOffset + "f");
        AddSpace();
        AddProperty("float", "Gravity", Gravity + "f");
        AddProperty("float", "FallLimit", FallLimit + "f");
        AddProperty("float", "SlopeLimit", SlopeLimit + "f");
        AddProperty("float", "WallLimit", WallLimit + "f");
        AddSpace();
        AddProperty("float", "DefaultGravityScale", DefaultGravityScale + "f");
        AddProperty("float", "FallGravityScale", FallGravityScale + "f");

        EndClass();
        EndNamespace();
        EndNamespace();
        File.WriteAllText(SCRIPT_PATH, EndScript());

        Save();
    }

    private void SetDefaultValues() {
        foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
            field.SetValue(this, GetType().GetField("Default" + field.Name, BindingFlags.Public | BindingFlags.Static).GetValue(null));
        }
    }


    private void BeginScript() { script = string.Empty; group = 0; }
    private string EndScript() { return script; }
    private void BeginNamespace(string name) { script += new string('\t', group++) + "namespace " + name + " { \n"; }
    private void EndNamespace() { script += new string('\t', --group) + "} \n"; }
    private void BeginClass(string name) { script += new string('\t', group++) + "public static class " + name + "{ \n"; }
    private void EndClass() { script += new string('\t', --group) + "} \n"; }
    private void AddProperty(string type, string name, string value) { script += new string('\t', group) + "public const " + type + " " + name + " = " + value + "; \n"; }
    private void AddSpace() { script += "\n"; }


    private void Load() {
        if (File.Exists(CONFIG_PATH)) {
            string json = File.ReadAllText(CONFIG_PATH);
            Dictionary<string, object> items = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields) {
                if (!items.TryGetValue(field.Name, out object value)) {
                    value = GetType().GetField("Default" + field.Name, BindingFlags.Public | BindingFlags.Static).GetValue(null);
                }
                if (value is double @double) {
                    field.SetValue(this, (float)@double);
                } else {
                    field.SetValue(this, value);
                }
            }
        } else {
            SetDefaultValues();
        }
    }
    private void Save() {
        Dictionary<string, object> items = new Dictionary<string, object>();

        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields) {
            items[field.Name] = field.GetValue(this);
        }

        string json = JsonConvert.SerializeObject(items);
        File.WriteAllText(CONFIG_PATH, json);
    }
}