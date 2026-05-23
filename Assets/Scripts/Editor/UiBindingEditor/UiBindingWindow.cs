using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GenBall.Utils.CodeGenerator.UI.Editor
{
    /// <summary>
    /// Editor window for scanning prefabs and generating UI View/Logic code.
    /// Menu: Window > UI Code Generator
    /// </summary>
    public class UiBindingWindow : EditorWindow
    {
        private GameObject _prefab;
        private UiBindingConfig _config;
        private UiPrefabScanner.ScanResult _scanResult;
        private string _formName = "";
        private string _formType = "Popup";
        private string _namespaceName = "GenBall.UI";
        private bool _generateView = true;
        private bool _generateLogic = true;
        private bool _forceOverwrite = false;
        private Vector2 _scrollPos;
        private string _statusMessage = "";

        private readonly string[] _formTypeOptions = { "Persistent", "Popup", "Transition" };

        [MenuItem("Window/UI Code Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<UiBindingWindow>("UI Code Generator");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            // Try to find the default config
            if (_config == null)
            {
                var guids = AssetDatabase.FindAssets("t:UiBindingConfig");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _config = AssetDatabase.LoadAssetAtPath<UiBindingConfig>(path);
                }
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("UI Code Generator", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Tip: For a faster workflow, attach a UiViewBinding component " +
                "to your prefab and use its Inspector directly — no need for this window.",
                MessageType.Info);

            GUILayout.Space(10);

            // -- Config --
            EditorGUILayout.BeginHorizontal();
            _config = (UiBindingConfig)EditorGUILayout.ObjectField(
                "Binding Config", _config, typeof(UiBindingConfig), false);
            if (GUILayout.Button("New", GUILayout.Width(50)))
            {
                CreateNewConfig();
            }
            EditorGUILayout.EndHorizontal();

            // -- Prefab --
            EditorGUI.BeginChangeCheck();
            _prefab = (GameObject)EditorGUILayout.ObjectField(
                "Source Prefab", _prefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck() && _prefab != null)
            {
                AutoScan();
            }

            GUILayout.Space(5);

            // -- Scan button --
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Scan Prefab", GUILayout.Height(30)))
            {
                AutoScan();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // -- Form Settings --
            if (_scanResult != null)
            {
                GUILayout.Label("Form Settings", EditorStyles.boldLabel);
                _formName = EditorGUILayout.TextField("Form Name", _formName);
                _formType = _formTypeOptions[
                    EditorGUILayout.Popup("Form Type",
                        Array.IndexOf(_formTypeOptions, _formType),
                        _formTypeOptions)];
                _namespaceName = EditorGUILayout.TextField("Namespace", _namespaceName);

                GUILayout.Space(5);

                // -- Detected Bindings --
                GUILayout.Label($"Bindings ({_scanResult.bindings.Count})", EditorStyles.boldLabel);

                if (_scanResult.bindings.Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No bindings detected. Make sure GameObject names match the prefix conventions " +
                        "in your UiBindingConfig (e.g., 'Btn*', 'Txt*', 'Img*').",
                        MessageType.Info);
                }
                else
                {
                    foreach (var b in _scanResult.bindings)
                    {
                        EditorGUILayout.BeginHorizontal();
                        b.included = EditorGUILayout.Toggle(b.included, GUILayout.Width(20));
                        var label = $"{b.propertyName}  [{b.componentType}]";
                        EditorGUILayout.LabelField(label);
                        EditorGUILayout.LabelField(b.childPath, EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                GUILayout.Space(5);

                // -- Warnings --
                if (_scanResult.warnings.Count > 0)
                {
                    GUILayout.Label($"Warnings ({_scanResult.warnings.Count})", EditorStyles.boldLabel);
                    foreach (var w in _scanResult.warnings)
                    {
                        EditorGUILayout.HelpBox(w, MessageType.Warning);
                    }
                }

                GUILayout.Space(10);

                // -- Generation Options --
                GUILayout.Label("Generation Options", EditorStyles.boldLabel);
                _generateView = EditorGUILayout.Toggle("Generate View", _generateView);
                _generateLogic = EditorGUILayout.Toggle("Generate Logic", _generateLogic);
                _forceOverwrite = EditorGUILayout.Toggle("Force Overwrite", _forceOverwrite);

                GUILayout.Space(10);

                // -- Generate Button --
                if (GUILayout.Button("Generate Code", GUILayout.Height(40)))
                {
                    GenerateCode();
                }

                // -- Status --
                if (!string.IsNullOrEmpty(_statusMessage))
                {
                    EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void AutoScan()
        {
            _statusMessage = "";

            if (_prefab == null) return;
            if (_config == null)
            {
                _statusMessage = "Please assign a UiBindingConfig.";
                return;
            }

            var prefabPath = AssetDatabase.GetAssetPath(_prefab);
            if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
            {
                _statusMessage = "Selected object is not a prefab asset. Drag a .prefab file from the Project view.";
                return;
            }

            _scanResult = UiPrefabScanner.Scan(_prefab, _config, prefabPath);
            _formName = _scanResult.formName;

            _statusMessage = $"Scanned: {_scanResult.bindings.Count} binding(s) found.";
        }

        private void GenerateCode()
        {
            if (_scanResult == null)
            {
                _statusMessage = "Please scan a prefab first.";
                return;
            }

            if (string.IsNullOrEmpty(_formName))
            {
                _statusMessage = "Form name cannot be empty.";
                return;
            }

            if (_config == null)
            {
                _statusMessage = "Please assign a UiBindingConfig.";
                return;
            }

            try
            {
                var outputDir = Path.Combine(_config.outputBasePath, _formName);

                if (_generateView)
                {
                    var code = UiBindingCodeGenerator.GenerateViewCode(
                        _formName,
                        _scanResult.prefabPath,
                        _scanResult.bindings,
                        _namespaceName,
                        _config.viewBaseClass);

                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    var path = Path.Combine(outputDir, UiBindingCodeGenerator.GetViewFileName(_formName));
                    if (File.Exists(path) && !_forceOverwrite)
                    {
                        Debug.LogWarning($"[UiCodeGenerator] Skipped (exists): {path}");
                    }
                    else
                    {
                        File.WriteAllText(path, code, System.Text.Encoding.UTF8);
                    }
                }

                if (_generateLogic)
                {
                    var code = UiBindingCodeGenerator.GenerateLogicCode(
                        _formName,
                        _scanResult.prefabPath,
                        _formType,
                        _namespaceName,
                        _config.logicBaseClass);

                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    var path = Path.Combine(outputDir, UiBindingCodeGenerator.GetLogicFileName(_formName));
                    if (File.Exists(path) && !_forceOverwrite)
                    {
                        Debug.LogWarning($"[UiCodeGenerator] Skipped (exists): {path}");
                    }
                    else
                    {
                        File.WriteAllText(path, code, System.Text.Encoding.UTF8);
                    }
                }

                AssetDatabase.Refresh();
                _statusMessage = $"Code generated successfully for {_formName}.\nOutput: {outputDir}";

                EditorUtility.DisplayDialog(
                    "Code Generation Complete",
                    $"Generated View and Logic for '{_formName}'.\n\nOutput directory:\n{outputDir}",
                    "OK");
            }
            catch (Exception e)
            {
                _statusMessage = $"Error: {e.Message}";
                Debug.LogError($"[UiCodeGenerator] Generation failed: {e}");
            }
        }

        private void CreateNewConfig()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Binding Config",
                "UiBindingConfig",
                "asset",
                "Create a new UI Binding Configuration asset.");
            if (string.IsNullOrEmpty(path)) return;

            var config = CreateInstance<UiBindingConfig>();

            // Populate with default mappings
            config.prefixMappings.Add(new PrefixMapping
            {
                prefix = "Btn", componentType = "Button", fullName = "UnityEngine.UI.Button",
                usingNamespace = "UnityEngine.UI", category = "interactive", priority = 10
            });
            config.prefixMappings.Add(new PrefixMapping
            {
                prefix = "Txt", componentType = "Text", fullName = "UnityEngine.UI.Text",
                usingNamespace = "UnityEngine.UI", category = "display", priority = 20
            });
            config.prefixMappings.Add(new PrefixMapping
            {
                prefix = "Img", componentType = "Image", fullName = "UnityEngine.UI.Image",
                usingNamespace = "UnityEngine.UI", category = "display", priority = 30
            });
            config.prefixMappings.Add(new PrefixMapping
            {
                prefix = "Rect", componentType = "RectTransform", fullName = "UnityEngine.RectTransform",
                usingNamespace = "UnityEngine", category = "layout", priority = 40
            });

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            _config = config;

            _statusMessage = $"Created new config at: {path}";
        }
    }
}
