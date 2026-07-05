using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GenBall.Utils.CodeGenerator.UI.Editor
{
    /// <summary>
    /// Custom Inspector for UiViewBinding.
    /// Provides full workflow: Scan, select output path, and Generate,
    /// all directly in the prefab Inspector.
    /// </summary>
    [CustomEditor(typeof(UiViewBinding))]
    public class UiViewBindingEditor : UnityEditor.Editor
    {
        private SerializedProperty _bindingConfigProp;
        private SerializedProperty _formNameProp;
        private SerializedProperty _formTypeProp;
        private SerializedProperty _namespaceProp;
        private SerializedProperty _outputPathProp;
        private SerializedProperty _generateTargetProp;
        private SerializedProperty _bindingsProp;

        private List<string> _warnings = new List<string>();
        private string _defaultOutputPath;
        private bool _showScanResults;
        private bool _showWarnings;
        private bool _showNamingReference = true;
        private Vector2 _scrollPos;

        #region Default Prefix Mappings

        private static readonly (string prefix, string componentType, string fullName, string ns, string cat, int pri)[]
            DefaultMappings =
            {
                ("Btn",  "Button",      "UnityEngine.UI.Button",               "UnityEngine.UI", "interactive", 10),
                ("Txt",  "Text",        "UnityEngine.UI.Text",                 "UnityEngine.UI", "display",     20),
                ("Img",  "Image",       "UnityEngine.UI.Image",                "UnityEngine.UI", "display",     30),
                ("RawImg","RawImage",   "UnityEngine.UI.RawImage",             "UnityEngine.UI", "display",     35),
                ("Rect", "RectTransform","UnityEngine.RectTransform",          "UnityEngine",    "layout",      40),
                ("Input","InputField",  "UnityEngine.UI.InputField",           "UnityEngine.UI", "interactive", 50),
                ("Slider","Slider",     "UnityEngine.UI.Slider",               "UnityEngine.UI", "interactive", 60),
                ("Toggle","Toggle",     "UnityEngine.UI.Toggle",               "UnityEngine.UI", "interactive", 70),
                ("Scroll","ScrollRect", "UnityEngine.UI.ScrollRect",           "UnityEngine.UI", "interactive", 80),
                ("Dropdown","Dropdown", "UnityEngine.UI.Dropdown",             "UnityEngine.UI", "interactive", 90),
                ("Scrollbar","Scrollbar","UnityEngine.UI.Scrollbar",           "UnityEngine.UI", "interactive",100),
                ("CanvasGroup","CanvasGroup","UnityEngine.CanvasGroup",        "UnityEngine",    "layout",     110),
                ("LayoutElem","LayoutElement","UnityEngine.UI.LayoutElement",  "UnityEngine.UI", "layout",     120),
                ("Fitter","ContentSizeFitter","UnityEngine.UI.ContentSizeFitter","UnityEngine.UI","layout",     130),
                ("HLayout","HorizontalLayoutGroup","UnityEngine.UI.HorizontalLayoutGroup","UnityEngine.UI","layout",140),
                ("VLayout","VerticalLayoutGroup","UnityEngine.UI.VerticalLayoutGroup","UnityEngine.UI","layout",150),
                ("Grid",  "GridLayoutGroup","UnityEngine.UI.GridLayoutGroup",  "UnityEngine.UI", "layout",     160),
            };

        #endregion

        private void OnEnable()
        {
            _bindingConfigProp = serializedObject.FindProperty("bindingConfig");
            _formNameProp = serializedObject.FindProperty("formName");
            _formTypeProp = serializedObject.FindProperty("formType");
            _namespaceProp = serializedObject.FindProperty("namespaceName");
            _outputPathProp = serializedObject.FindProperty("outputPath");
            _generateTargetProp = serializedObject.FindProperty("generateTarget");
            _bindingsProp = serializedObject.FindProperty("bindings");

            UpdateDefaultPath();
        }

        private void UpdateDefaultPath()
        {
            var binding = (UiViewBinding)target;
            var formName = string.IsNullOrEmpty(binding.formName)
                ? binding.gameObject.name
                : binding.formName;
            _defaultOutputPath = $"Assets/Scripts/GenBall/UI/{formName}";
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var binding = (UiViewBinding)target;

            // -- Config --
            EditorGUILayout.PropertyField(_bindingConfigProp, new GUIContent("Binding Config"));
            if (binding.bindingConfig == null)
            {
                EditorGUILayout.HelpBox(
                    "No Binding Config assigned. A default config with 17 prefix mappings will be used.",
                    MessageType.Info);
            }

            // -- View Type --
            var viewTypeProp = serializedObject.FindProperty("viewType");
            EditorGUILayout.PropertyField(viewTypeProp, new GUIContent("View Type"));
            if (binding.viewType == UiViewBinding.ViewType.Part)
            {
                EditorGUILayout.HelpBox(
                    "Part mode: this component is a reusable UI sub-component.\n" +
                    "Parent Forms will NOT scan into this Part's children.",
                    MessageType.Info);
            }

            // -- Naming Convention Reference --
            DrawNamingConventionReference(binding);

            EditorGUILayout.Space();

            // -- Form Settings --
            GUILayout.Label("Form Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_formNameProp);
            if (EditorGUI.EndChangeCheck())
                UpdateDefaultPath();

            if (string.IsNullOrEmpty(binding.formName) && !string.IsNullOrEmpty(binding.gameObject.name))
            {
                EditorGUILayout.HelpBox(
                    $"Form name will default to: \"{binding.gameObject.name}\"",
                    MessageType.Info);
            }

            EditorGUILayout.PropertyField(_formTypeProp);
            EditorGUILayout.PropertyField(_namespaceProp);

            EditorGUILayout.Space();

            // -- Output Path --
            GUILayout.Label("Output Path", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_outputPathProp, new GUIContent("Path"));
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var currentPath = string.IsNullOrEmpty(binding.outputPath)
                    ? _defaultOutputPath
                    : binding.outputPath;
                var selected = EditorUtility.OpenFolderPanel("Select Output Directory", currentPath, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    // Convert absolute path to project-relative
                    var dataPath = Application.dataPath;
                    if (selected.StartsWith(dataPath))
                    {
                        binding.outputPath = "Assets" + selected.Substring(dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Path",
                            "Please select a folder inside the project's Assets directory.",
                            "OK");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Reset to Default", GUILayout.Width(120)))
            {
                binding.outputPath = "";
            }

            var displayPath = string.IsNullOrEmpty(binding.outputPath)
                ? _defaultOutputPath
                : binding.outputPath;
            var pathStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };
            EditorGUILayout.LabelField($"  → {displayPath}", pathStyle);

            EditorGUILayout.Space();

            // -- Scan Button --
            var scanColor = GUI.color;
            GUI.color = new Color(0.6f, 0.8f, 1f);
            if (GUILayout.Button("Scan Bindings", GUILayout.Height(35)))
            {
                ScanBindings(binding);
            }
            GUI.color = scanColor;

            EditorGUILayout.Space();

            // -- Scan Results --
            if (binding.bindings.Count > 0)
            {
                _showScanResults = EditorGUILayout.Foldout(_showScanResults,
                    $"Detected Bindings ({binding.bindings.Count(b => b.included)}/{binding.bindings.Count})", true);
                if (_showScanResults)
                {
                    EditorGUI.indentLevel++;
                    foreach (var entry in binding.bindings)
                    {
                        EditorGUILayout.BeginHorizontal();
                        entry.included = EditorGUILayout.Toggle(entry.included, GUILayout.Width(18));
                        EditorGUILayout.LabelField(
                            $"{entry.propertyName}  [{entry.componentType}]",
                            EditorStyles.boldLabel);
                        var childStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            normal = { textColor = Color.gray },
                            alignment = TextAnchor.MiddleRight
                        };
                        EditorGUILayout.LabelField(entry.childPath, childStyle,
                            GUILayout.Width(180));
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
            }

            // -- Warnings --
            if (_warnings.Count > 0)
            {
                _showWarnings = EditorGUILayout.Foldout(_showWarnings,
                    $"Warnings ({_warnings.Count})", true);
                if (_showWarnings)
                {
                    EditorGUI.indentLevel++;
                    foreach (var w in _warnings)
                        EditorGUILayout.HelpBox(w, MessageType.Warning);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Space();

            // -- Generate --
            EditorGUILayout.PropertyField(_generateTargetProp);

            EditorGUILayout.Space();

            var genColor = GUI.color;
            GUI.color = new Color(0.4f, 0.9f, 0.4f);
            if (GUILayout.Button("Generate Code", GUILayout.Height(40)))
            {
                GenerateCode(binding);
            }
            GUI.color = genColor;

            serializedObject.ApplyModifiedProperties();
        }

        private UiBindingConfig GetEffectiveConfig(UiViewBinding binding)
        {
            if (binding.bindingConfig != null)
                return binding.bindingConfig;

            // Try to find one in the project
            var guids = AssetDatabase.FindAssets("t:UiBindingConfig");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                binding.bindingConfig = AssetDatabase.LoadAssetAtPath<UiBindingConfig>(path);
                return binding.bindingConfig;
            }

            return null;
        }

        private List<(string, string, string, string, string, int)> GetPrefixMappings(UiViewBinding binding)
        {
            var config = GetEffectiveConfig(binding);
            if (config != null && config.prefixMappings.Count > 0)
            {
                return config.prefixMappings
                    .Select(m => (m.prefix, m.componentType, m.fullName, m.usingNamespace, m.category, m.priority))
                    .ToList();
            }

            // Fallback to hardcoded defaults
            return new List<(string, string, string, string, string, int)>(DefaultMappings);
        }

        private void ScanBindings(UiViewBinding binding)
        {
            _warnings.Clear();

            var go = binding.gameObject;
            var mappings = GetPrefixMappings(binding);

            // Sort by prefix length descending (longest match first)
            mappings.Sort((a, b) => b.Item1.Length.CompareTo(a.Item1.Length));

            binding.bindings.Clear();

            var allTransforms = go.GetComponentsInChildren<Transform>(includeInactive: true);

            // -- Part boundary: find child UiViewBinding roots and skip their subtrees --
            var partSubtrees = new HashSet<Transform>();
            foreach (var t in allTransforms)
            {
                if (t == go.transform) continue;
                var childGo = t.gameObject;
                if (childGo.TryGetComponent<UiViewBinding>(out _))
                {
                    // Collect all descendants of this Part root (including itself)
                    foreach (var sub in childGo.GetComponentsInChildren<Transform>(includeInactive: true))
                        partSubtrees.Add(sub);
                }
            }

            var dedupNames = new Dictionary<string, int>();

            foreach (var t in allTransforms)
            {
                if (t == go.transform) continue; // skip root
                if (partSubtrees.Contains(t)) continue; // belongs to a Part — skip

                var childGo = t.gameObject;
                var name = childGo.name;

                // Find matching prefix
                var match = mappings.FirstOrDefault(m => name.StartsWith(m.Item1, StringComparison.Ordinal));
                if (match == default) continue;

                var (prefix, componentType, fullName, usingNs, category, priority) = match;

                // Find the component type
                var compType = FindType(fullName);
                if (compType == null)
                    continue;

                var comp = childGo.GetComponent(compType);
                if (comp == null)
                {
                    _warnings.Add($"'{name}' matches prefix '{prefix}' but has no {componentType} component.");
                    continue;
                }

                // Derive property name
                var propName = SanitizeName(name);
                if (dedupNames.ContainsKey(propName))
                {
                    dedupNames[propName]++;
                    propName = $"{propName}_{dedupNames[propName]}";
                }
                else
                {
                    dedupNames[propName] = 1;
                }

                var childPath = AnimationUtility.CalculateTransformPath(t, go.transform);

                binding.bindings.Add(new UiViewBinding.BindingEntry
                {
                    gameObjectName = name,
                    componentType = componentType,
                    fullTypeName = fullName,
                    propertyName = propName,
                    childPath = childPath,
                    component = comp,
                    included = true
                });
            }

            // Check for un-prefixed GameObjects with UI components
            var allMappedNames = new HashSet<string>();
            foreach (var m in mappings)
                allMappedNames.Add(m.Item2); // componentType short name

            foreach (var t in allTransforms)
            {
                if (t == go.transform) continue;
                var childGo = t.gameObject;
                var name = childGo.name;

                // Already matched by prefix
                if (mappings.Any(m => name.StartsWith(m.Item1, StringComparison.Ordinal)))
                    continue;

                // Check for bindable components
                var found = new List<string>();
                foreach (var (prefix, shortType, fullName, ns, cat, pri) in mappings)
                {
                    var type = FindType(fullName);
                    if (type != null && childGo.GetComponent(type) != null)
                        found.Add(shortType);
                }

                if (found.Count > 0)
                {
                    // Skip Unity default names
                    if (name.StartsWith("Text") || name.StartsWith("Image") ||
                        name.StartsWith("Button") || name.StartsWith("RawImage") ||
                        name.StartsWith("GameObject") || name.StartsWith("Canvas") ||
                        name.StartsWith("Panel") || name.StartsWith("Content") ||
                        name.StartsWith("Viewport") || name.StartsWith("Scrollbar") ||
                        name.StartsWith("Placeholder") || name.StartsWith("Label") ||
                        name.StartsWith("Background") || name.StartsWith("Checkmark") ||
                        name.StartsWith("Handle"))
                        continue;

                    _warnings.Add(
                        $"'{name}' has UI components ({string.Join(", ", found)}) but no binding prefix.");
                }
            }

            EditorUtility.SetDirty(binding);
            Debug.Log($"[UiCodeGenerator] Scan complete: {binding.bindings.Count} binding(s) found.");
        }

        private void GenerateCode(UiViewBinding binding)
        {
            if (binding.bindings.Count == 0)
            {
                if (EditorUtility.DisplayDialog("No Bindings",
                        "No bindings found. Run Scan first?\n\n(Click 'Scan Bindings' button above.)",
                        "OK"))
                    return;
            }

            var formName = string.IsNullOrEmpty(binding.formName)
                ? binding.gameObject.name
                : binding.formName;

            // Get prefab path: try asset path first (Prefab Mode),
            // then fall back to PrefabStage (also Prefab Mode, some Unity versions).
            var prefabPath = AssetDatabase.GetAssetPath(binding.gameObject);
            if (string.IsNullOrEmpty(prefabPath))
            {
                var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (stage != null)
                    prefabPath = stage.assetPath;
            }

            if (string.IsNullOrEmpty(prefabPath))
            {
                EditorUtility.DisplayDialog("Error",
                    "This component must be on a prefab asset.\nOpen the prefab (double-click it in Project view) and try again.",
                    "OK");
                return;
            }

            var outputDir = string.IsNullOrEmpty(binding.outputPath)
                ? _defaultOutputPath
                : binding.outputPath;

            var formType = binding.formType.ToString();
            var ns = binding.namespaceName;
            var viewType = binding.viewType;

            try
            {
                ActiveEditorTracker.sharedTracker.ForceRebuild();

                // 1. Auto-fix prefab components (Forms get UIFormScript; Parts skip it)
                UiBindingCodeGenerator.EnsurePrefabComponents(binding.gameObject, formType, viewType);

                // Select base classes based on view type
                string viewBase, logicBase;
                if (viewType == UiViewBinding.ViewType.Part)
                {
                    viewBase = binding.bindingConfig != null && !string.IsNullOrEmpty(binding.bindingConfig.partViewBaseClass)
                        ? binding.bindingConfig.partViewBaseClass
                        : "Yueyn.UI.PartViewBase";
                    logicBase = binding.bindingConfig != null
                        ? binding.bindingConfig.partLogicBaseClass
                        : "";
                }
                else
                {
                    viewBase = binding.bindingConfig != null
                        ? binding.bindingConfig.viewBaseClass
                        : "Yueyn.UI.UIBusinessFormBase";
                    logicBase = binding.bindingConfig != null
                        ? binding.bindingConfig.logicBaseClass
                        : "Yueyn.UI.BusinessFormLogic";
                }

                // Convert BindingEntry list to scanner format
                var scannerBindings = binding.bindings.Select(b => new UiPrefabScanner.BindingInfo
                {
                    gameObjectName = b.gameObjectName,
                    componentType = b.componentType,
                    fullTypeName = b.fullTypeName,
                    propertyName = b.propertyName,
                    childPath = b.childPath,
                    targetComponent = b.component,
                    included = b.included,
                }).ToList();

                var target = binding.generateTarget;
                var sb = new StringBuilder();

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                if (target == UiViewBinding.GenerateTarget.Both || target == UiViewBinding.GenerateTarget.ViewOnly)
                {
                    var viewBindingCode = UiBindingCodeGenerator.GenerateViewBindingCode(
                        formName, scannerBindings, viewType);
                    var viewPath = Path.Combine(outputDir, UiBindingCodeGenerator.GetViewFileName(formName));

                    UiBindingCodeGenerator.InjectOrCreateFile(viewPath, viewBindingCode, path =>
                    {
                        var template = UiBindingCodeGenerator.CreateViewFileTemplate(
                            formName, ns, scannerBindings, viewBase);
                        File.WriteAllText(path, template, Encoding.UTF8);
                        Debug.Log($"[UiCodeGenerator] Created View template: {path}");
                    });
                    sb.AppendLine($"View:  {viewPath}");

                    // ViewData template (once)
                    var viewDataPath = Path.Combine(outputDir, UiBindingCodeGenerator.GetViewDataFileName(formName));
                    if (!File.Exists(viewDataPath))
                    {
                        var vdTemplate = UiBindingCodeGenerator.CreateViewDataTemplate(formName, ns);
                        File.WriteAllText(viewDataPath, vdTemplate, Encoding.UTF8);
                        sb.AppendLine($"ViewData: {viewDataPath} (template)");
                    }
                }

                if (target == UiViewBinding.GenerateTarget.Both || target == UiViewBinding.GenerateTarget.LogicOnly)
                {
                    if (viewType == UiViewBinding.ViewType.Part && string.IsNullOrEmpty(logicBase))
                    {
                        sb.AppendLine("Logic: (skipped — Part has no logic base class configured)");
                    }
                    else
                    {
                        var logicBindingCode = UiBindingCodeGenerator.GenerateLogicBindingCode(
                            formName, prefabPath, formType, viewType);
                        var logicPath = Path.Combine(outputDir, UiBindingCodeGenerator.GetLogicFileName(formName));

                        UiBindingCodeGenerator.InjectOrCreateFile(logicPath, logicBindingCode, path =>
                        {
                            var template = UiBindingCodeGenerator.CreateLogicFileTemplate(
                                formName, ns, logicBase, viewType);
                            File.WriteAllText(path, template, Encoding.UTF8);
                            Debug.Log($"[UiCodeGenerator] Created Logic template: {path}");
                        });
                        sb.AppendLine($"Logic: {logicPath}");
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log($"[UiCodeGenerator] Generated for {formName}:\n{sb}");

                EditorUtility.DisplayDialog("Code Generated",
                    $"Generated for \"{formName}\":\n\n{sb}",
                    "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Generation Failed", ex.Message, "OK");
                Debug.LogError($"[UiCodeGenerator] {ex}");
            }
        }

        #region Naming Convention Reference

        private void DrawNamingConventionReference(UiViewBinding binding)
        {
            _showNamingReference = EditorGUILayout.Foldout(_showNamingReference,
                "Naming Convention Reference", true);
            if (!_showNamingReference)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(
                "Rule: [Prefix][DescriptiveName], e.g. \"BtnStart\", \"TxtTitle\"",
                EditorStyles.miniLabel);

            var mappings = GetPrefixMappings(binding);
            var categories = mappings
                .GroupBy(m => m.Item5) // category
                .OrderBy(g => g.First().Item6) // lowest priority per group
                .ToList();

            foreach (var group in categories)
            {
                EditorGUILayout.Space(3);
                var catName = char.ToUpperInvariant(group.Key[0]) + group.Key.Substring(1);
                EditorGUILayout.LabelField(catName, EditorStyles.boldLabel);

                foreach (var m in group.OrderBy(m => m.Item6))
                {
                    var (prefix, compType, fullName, usingNs, category, priority) = m;
                    var example = $"{prefix}Start";
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    EditorGUILayout.LabelField($"{prefix}", GUILayout.Width(80));
                    EditorGUILayout.LabelField("→", GUILayout.Width(15));
                    EditorGUILayout.LabelField(compType, GUILayout.MinWidth(80));

                    var exampleStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        normal = { textColor = Color.gray }
                    };
                    EditorGUILayout.LabelField($"e.g. \"{example}\"", exampleStyle);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        #endregion

        #region Helpers

        private static Type FindType(string fullName)
        {
            var type = Type.GetType($"{fullName}, UnityEngine.UI");
            if (type != null) return type;

            type = Type.GetType($"{fullName}, UnityEngine");
            if (type != null) return type;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }

        private static string SanitizeName(string name)
        {
            var chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_')
                    chars[i] = '_';

            var result = new string(chars);
            if (result.Length > 0 && char.IsDigit(result[0]))
                result = "_" + result;
            return result;
        }

        #endregion
    }
}
