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
    /// Generates View.cs and Logic.cs C# code from scanned bindings.
    ///
    /// Injection marker mode:
    /// - Generated code is injected between START_MARKER and END_MARKER comments
    /// - Code outside markers is user-controlled and never modified
    /// - If the target file doesn't exist, a full template is created first
    /// </summary>
    public static class UiBindingCodeGenerator
    {
        public const string START_MARKER = "// ### GENERATED_BINDINGS_START ###";
        public const string END_MARKER = "// ### GENERATED_BINDINGS_END ###";

        // ── View binding code (marker region content only) ──

        public static string GenerateViewBindingCode(
            string formName,
            List<UiPrefabScanner.BindingInfo> bindings,
            UiViewBinding.ViewType viewType = UiViewBinding.ViewType.Form)
        {
            var sb = new StringBuilder();
            var included = bindings.Where(b => b.included).ToList();

            sb.AppendLine($"        private UiViewBinding _binding;");
            sb.AppendLine();

            if (included.Count == 0)
            {
                sb.AppendLine("        // (no bindings detected)");
            }
            else
            {
                foreach (var b in included)
                    sb.AppendLine($"        public {b.componentType} {b.propertyName} {{ get; private set; }}");
            }

            sb.AppendLine();
            sb.AppendLine("        private void BindControls()");
            sb.AppendLine("        {");

            if (included.Count == 0)
            {
                sb.AppendLine("            // No controls to bind");
            }
            else
            {
                sb.AppendLine("            _binding = GetComponent<UiViewBinding>();");
                int maxLen = included.Max(b => b.propertyName.Length);
                foreach (var b in included)
                {
                    var pad = new string(' ', maxLen - b.propertyName.Length);
                    sb.AppendLine(
                        $"            {b.propertyName}{pad} = _binding.GetBinding<{b.componentType}>(\"{b.propertyName}\");");
                }
            }

            sb.AppendLine("        }");

            return sb.ToString();
        }

        // ── Logic binding code (marker region content only) ──

        public static string GenerateLogicBindingCode(
            string formName,
            string prefabPath,
            string formType,
            UiViewBinding.ViewType viewType = UiViewBinding.ViewType.Form)
        {
            var sb = new StringBuilder();

            if (viewType == UiViewBinding.ViewType.Form)
            {
                sb.AppendLine($"        public override string PrefabPath =>");
                sb.AppendLine($"            \"{prefabPath}\";");
                sb.AppendLine();
                sb.AppendLine($"        public override UIFormType FormType => UIFormType.{formType};");
                sb.AppendLine();
                sb.AppendLine($"        public {formName}View View {{ get; private set; }}");
            }
            else
            {
                // Part logic: PrefabPath is for dynamic instantiation (static Parts ignore it).
                // BoundView is inherited from BusinessPartLogic<TView> — no separate View property needed.
                sb.AppendLine($"        public override string PrefabPath =>");
                sb.AppendLine($"            \"{prefabPath}\";");
            }

            return sb.ToString();
        }

        // ── File templates (created when target file doesn't exist) ──

        public static string CreateViewFileTemplate(
            string formName,
            string namespaceName,
            List<UiPrefabScanner.BindingInfo> bindings,
            string baseClass = "Yueyn.UI.UIBusinessFormBase")
        {
            var sb = new StringBuilder();
            var viewDataClassName = $"{formName}ViewData";

            // Construct generic base: UIBusinessFormBase<XxxViewData> or PartViewBase<XxxViewData>
            string shortBase;
            if (baseClass.Contains("PartViewBase"))
                shortBase = $"PartViewBase<{viewDataClassName}>";
            else
                shortBase = $"UIBusinessFormBase<{viewDataClassName}>";

            // Collect usings from bindings
            var neededUsings = new HashSet<string> { "UnityEngine", "GenBall.Utils.CodeGenerator.UI", "Yueyn.UI" };
            foreach (var b in bindings)
            {
                var ns = GetNamespace(b.fullTypeName);
                if (!string.IsNullOrEmpty(ns) && ns != "UnityEngine")
                    neededUsings.Add(ns);
            }

            foreach (var ns in neededUsings.OrderBy(n => n))
                sb.AppendLine($"using {ns};");

            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {formName}View : {shortBase}");
            sb.AppendLine("    {");
            sb.AppendLine($"        {START_MARKER}");
            sb.AppendLine($"        {END_MARKER}");
            sb.AppendLine();
            sb.AppendLine("        protected override void DoBusinessStart()");
            sb.AppendLine("        {");
            sb.AppendLine("            base.DoBusinessStart();");
            sb.AppendLine("            BindControls();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected override void RefreshView()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: 在此处实现 View 刷新逻辑");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();

            return sb.ToString();
        }

        public static string CreateLogicFileTemplate(
            string formName,
            string namespaceName,
            string baseClass = "Yueyn.UI.BusinessFormLogic",
            UiViewBinding.ViewType viewType = UiViewBinding.ViewType.Form)
        {
            string shortBase;
            if (viewType == UiViewBinding.ViewType.Part)
                shortBase = $"BusinessPartLogic<{formName}View>";
            else
                shortBase = baseClass.Split('.').Last();

            var sb = new StringBuilder();

            if (baseClass.StartsWith("Yueyn"))
                sb.AppendLine("using Yueyn.UI;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {formName}Logic : {shortBase}");
            sb.AppendLine("    {");
            sb.AppendLine($"        {START_MARKER}");
            sb.AppendLine($"        {END_MARKER}");
            sb.AppendLine();

            if (viewType == UiViewBinding.ViewType.Form)
            {
                sb.AppendLine("        protected override void OnFormCreated()");
                sb.AppendLine("        {");
                sb.AppendLine("            base.OnFormCreated();");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        protected override void OnFormBound(UIFormScript form)");
                sb.AppendLine("        {");
                sb.AppendLine("            base.OnFormBound(form);");
                sb.AppendLine($"            View = form.GetComponentInChildren<{formName}View>();");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        protected override void OnFormUnbound(UIFormScript form)");
                sb.AppendLine("        {");
                sb.AppendLine("            View = null;");
                sb.AppendLine("            base.OnFormUnbound(form);");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine($"        public static {formName}Logic Open()");
                sb.AppendLine("        {");
                sb.AppendLine($"            return BusinessLogicManager.Instance.CreateLogic<{formName}Logic>();");
                sb.AppendLine("        }");
            }
            else
            {
                sb.AppendLine("        protected override void OnPartCreated()");
                sb.AppendLine("        {");
                sb.AppendLine("            base.OnPartCreated();");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        protected override void OnViewBound(PartViewBase view)");
                sb.AppendLine("        {");
                sb.AppendLine("            base.OnViewBound(view);");
                sb.AppendLine("            // Use BoundView (typed via generic) to access the PartView");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        protected override void OnViewUnbound(PartViewBase view)");
                sb.AppendLine("        {");
                sb.AppendLine("            base.OnViewUnbound(view);");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine($"        public static {formName}Logic Create({formName}View partView)");
                sb.AppendLine("        {");
                sb.AppendLine($"            return BusinessLogicManager.Instance.CreateLogic<{formName}Logic>(");
                sb.AppendLine($"                p => p.ParentTransform = partView.transform);");
                sb.AppendLine("        }");
            }

            sb.AppendLine();
            sb.AppendLine("        // 在此处添加业务逻辑...");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();

            return sb.ToString();
        }

        public static string CreateViewDataTemplate(
            string formName,
            string namespaceName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {formName}ViewData");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 在此处定义 View 数据字段");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            return sb.ToString();
        }

        // ── File injection ──

        /// <summary>
        /// Inject binding code between markers in an existing file, or create the file from template.
        /// </summary>
        public static void InjectOrCreateFile(
            string filePath,
            string bindingCode,
            Action<string> createTemplate)
        {
            if (File.Exists(filePath))
            {
                var content = File.ReadAllText(filePath, Encoding.UTF8);
                var startIdx = content.IndexOf(START_MARKER, StringComparison.Ordinal);
                var endIdx = content.IndexOf(END_MARKER, StringComparison.Ordinal);

                if (startIdx >= 0 && endIdx > startIdx)
                {
                    // Replace content between markers
                    var before = content.Substring(0, startIdx + START_MARKER.Length);
                    var after = content.Substring(endIdx);

                    // Ensure proper indentation: binding code lines already have 8-space indent
                    var newContent = before + Environment.NewLine + bindingCode + Environment.NewLine + "        " + after;
                    File.WriteAllText(filePath, newContent, Encoding.UTF8);
                    Debug.Log($"[UiCodeGenerator] Injected bindings into: {filePath}");
                }
                else
                {
                    Debug.LogError($"[UiCodeGenerator] Markers not found in '{filePath}'. "
                        + $"File must contain '{START_MARKER}' and '{END_MARKER}' on separate lines.");
                }
            }
            else
            {
                // Create template first, then inject
                createTemplate(filePath);
                AssetDatabase.Refresh();
                // Re-read and inject
                if (File.Exists(filePath))
                {
                    var content = File.ReadAllText(filePath, Encoding.UTF8);
                    var startIdx = content.IndexOf(START_MARKER, StringComparison.Ordinal);
                    var endIdx = content.IndexOf(END_MARKER, StringComparison.Ordinal);

                    if (startIdx >= 0 && endIdx > startIdx)
                    {
                        var before = content.Substring(0, startIdx + START_MARKER.Length);
                        var after = content.Substring(endIdx);
                        var newContent = before + Environment.NewLine + bindingCode + Environment.NewLine + "        " + after;
                        File.WriteAllText(filePath, newContent, Encoding.UTF8);
                    }
                }
            }
        }

        // ── File name helpers ──

        public static string GetViewFileName(string formName) => $"{formName}View.cs";
        public static string GetLogicFileName(string formName) => $"{formName}Logic.cs";
        public static string GetViewDataFileName(string formName) => $"{formName}ViewData.cs";

        // ── Generate All ──

        public static void GenerateAll(
            UiPrefabScanner.ScanResult scanResult,
            string formName,
            string formType,
            string outputDir,
            string namespaceName = "GenBall.UI",
            string viewBaseClass = "Yueyn.UI.UIBusinessFormBase",
            string logicBaseClass = "Yueyn.UI.BusinessFormLogic",
            UiViewBinding.ViewType viewType = UiViewBinding.ViewType.Form,
            bool force = false)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var sb = new StringBuilder();

            // ── View file ──
            var viewBindingCode = GenerateViewBindingCode(formName, scanResult.bindings, viewType);
            var viewPath = Path.Combine(outputDir, GetViewFileName(formName));

            InjectOrCreateFile(viewPath, viewBindingCode, path =>
            {
                var template = CreateViewFileTemplate(formName, namespaceName, scanResult.bindings, viewBaseClass);
                File.WriteAllText(path, template, Encoding.UTF8);
                Debug.Log($"[UiCodeGenerator] Created View template: {path}");
            });
            sb.AppendLine($"View:  {viewPath}");

            // ── Logic file ──
            if (!(viewType == UiViewBinding.ViewType.Part && string.IsNullOrEmpty(logicBaseClass)))
            {
                var logicBindingCode = GenerateLogicBindingCode(formName, scanResult.prefabPath, formType, viewType);
                var logicPath = Path.Combine(outputDir, GetLogicFileName(formName));

                InjectOrCreateFile(logicPath, logicBindingCode, path =>
                {
                    var template = CreateLogicFileTemplate(formName, namespaceName, logicBaseClass, viewType);
                    File.WriteAllText(path, template, Encoding.UTF8);
                    Debug.Log($"[UiCodeGenerator] Created Logic template: {path}");
                });
                sb.AppendLine($"Logic: {logicPath}");
            }

            // ── ViewData template (once) ──
            if (viewType == UiViewBinding.ViewType.Form || viewType == UiViewBinding.ViewType.Part)
            {
                var viewDataPath = Path.Combine(outputDir, GetViewDataFileName(formName));
                if (!File.Exists(viewDataPath))
                {
                    var template = CreateViewDataTemplate(formName, namespaceName);
                    File.WriteAllText(viewDataPath, template, Encoding.UTF8);
                    sb.AppendLine($"ViewData: {viewDataPath} (template)");
                }
            }

            AssetDatabase.Refresh();

            Debug.Log($"[UiCodeGenerator] Generated for {formName}:\n{sb}");
        }

        // ── Prefab component auto-fix ──

        /// <summary>
        /// Ensure the prefab root has the required components.
        /// Forms get UIFormScript + UiViewBinding; Parts only get UiViewBinding.
        /// </summary>
        public static void EnsurePrefabComponents(GameObject prefabRoot, string formTypeStr,
            UiViewBinding.ViewType viewType = UiViewBinding.ViewType.Form)
        {
            bool modified = false;

            // UIFormScript is only needed for Forms; Parts are managed by their parent Form
            if (viewType != UiViewBinding.ViewType.Part)
            {
                if (prefabRoot.GetComponent<Yueyn.UI.UIFormScript>() == null)
                {
                    prefabRoot.AddComponent<Yueyn.UI.UIFormScript>();
                    modified = true;
                    Debug.Log($"[UiCodeGenerator] Added UIFormScript to prefab root.");
                }
            }

            var binding = prefabRoot.GetComponent<GenBall.Utils.CodeGenerator.UI.UiViewBinding>();
            if (binding == null)
            {
                binding = prefabRoot.AddComponent<GenBall.Utils.CodeGenerator.UI.UiViewBinding>();
                modified = true;
                Debug.Log($"[UiCodeGenerator] Added UiViewBinding to prefab root.");
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefabRoot);
                AssetDatabase.SaveAssets();
            }
        }

        // ── Helpers ──

        private static string GetNamespace(string fullTypeName)
        {
            var lastDot = fullTypeName.LastIndexOf('.');
            if (lastDot <= 0) return "";
            return fullTypeName.Substring(0, lastDot);
        }
    }
}
