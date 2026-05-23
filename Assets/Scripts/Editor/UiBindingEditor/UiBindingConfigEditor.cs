using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GenBall.Utils.CodeGenerator.UI.Editor
{
    /// <summary>
    /// Custom editor for UiBindingConfig ScriptableObject.
    /// Allows editing prefix mappings and importing/exporting JSON.
    /// </summary>
    [CustomEditor(typeof(UiBindingConfig))]
    public class UiBindingConfigEditor : UnityEditor.Editor
    {
        private bool _showPrefixMappings = true;
        private bool _showGenerationSettings = true;
        private Vector2 _scrollPos;

        public override void OnInspectorGUI()
        {
            var config = (UiBindingConfig)target;

            serializedObject.Update();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // -- Prefix Mappings --
            _showPrefixMappings = EditorGUILayout.Foldout(_showPrefixMappings,
                $"Prefix Mappings ({config.prefixMappings.Count})", true);
            if (_showPrefixMappings)
            {
                EditorGUI.indentLevel++;

                var mappingsProp = serializedObject.FindProperty("prefixMappings");
                for (int i = 0; i < mappingsProp.arraySize; i++)
                {
                    var element = mappingsProp.GetArrayElementAtIndex(i);
                    var prefixProp = element.FindPropertyRelative("prefix");
                    var typeProp = element.FindPropertyRelative("componentType");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(element, new GUIContent(
                        $"{prefixProp.stringValue} → {typeProp.stringValue}"), false);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        mappingsProp.DeleteArrayElementAtIndex(i);
                        i--; // adjust index after deletion
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Mapping"))
                {
                    mappingsProp.arraySize++;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(5);

            // -- Generation Settings --
            _showGenerationSettings = EditorGUILayout.Foldout(_showGenerationSettings,
                "Generation Settings", true);
            if (_showGenerationSettings)
            {
                EditorGUI.indentLevel++;
                GUILayout.Label("Form", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("viewBaseClass"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("logicBaseClass"));
                EditorGUILayout.Space();
                GUILayout.Label("Part", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("partViewBaseClass"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("partLogicBaseClass"));
                EditorGUILayout.Space();
                GUILayout.Label("Output", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultNamespace"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("outputBasePath"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);

            // -- JSON Import/Export --
            GUILayout.Label("JSON Import/Export", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export JSON"))
            {
                ExportJson(config);
            }
            if (GUILayout.Button("Import JSON"))
            {
                ImportJson(config);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void ExportJson(UiBindingConfig config)
        {
            var json = config.ExportToJson();
            var path = EditorUtility.SaveFilePanel(
                "Export Binding Config JSON",
                "Assets",
                "bindings_export.json",
                "json");

            if (string.IsNullOrEmpty(path)) return;

            File.WriteAllText(path, json, Encoding.UTF8);
            AssetDatabase.Refresh();

            Debug.Log($"[UiCodeGenerator] Config exported to: {path}");
            EditorUtility.DisplayDialog("Export Complete",
                $"Configuration exported to:\n{path}", "OK");
        }

        private void ImportJson(UiBindingConfig config)
        {
            var path = EditorUtility.OpenFilePanel(
                "Import Binding Config JSON",
                "Assets",
                "json");

            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var json = File.ReadAllText(path, Encoding.UTF8);

                // Simple JSON parsing (no Newtonsoft dependency)
                // Wraps in an object for JsonUtility compatibility
                var wrapper = JsonUtility.FromJson<BindingConfigWrapper>("{\"items\":" + json + "}");

                if (wrapper?.items?.prefixMappings != null)
                {
                    config.prefixMappings = new System.Collections.Generic.List<PrefixMapping>(
                        wrapper.items.prefixMappings);
                }

                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();

                Debug.Log($"[UiCodeGenerator] Config imported from: {path}");
                EditorUtility.DisplayDialog("Import Complete",
                    $"Configuration imported from:\n{path}", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Import Failed",
                    $"Failed to import JSON:\n{e.Message}", "OK");
                Debug.LogError($"[UiCodeGenerator] Import failed: {e}");
            }
        }

        [System.Serializable]
        private class BindingConfigWrapper
        {
            public BindingConfigData items;
        }

        [System.Serializable]
        private class BindingConfigData
        {
            public PrefixMapping[] prefixMappings;
        }
    }
}
