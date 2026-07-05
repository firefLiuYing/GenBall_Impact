using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GenBall.Utils.CodeGenerator.UI;
using GenBall.Utils.CodeGenerator.UI.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace Yueyn.Editor.UnityMcp
{
    /// <summary>
    /// Compile state machine (file-based, survives domain reload):
    ///
    ///   <b>Incremental (default)</b> — fast, no unnecessary reload:
    ///   Compile() ──RequestScriptCompilation()──→ "compiling"
    ///                                                   │
    ///                    ┌──────────────────────────────┴──────────────┐
    ///                    ↓                                              ↓
    ///              Unity starts compiling                      Unity skips (no delta)
    ///                    │                                              │
    ///                    ↓                                              ↓
    ///                 "done"                                     "no_changes"
    ///
    ///   <b>Full rebuild (fullRebuild: true)</b> — comprehensive:
    ///   Compile() ──touch sentinel──→ "compiling"
    ///                    │
    ///   TriggerFullRebuild() ──AssetDatabase.Refresh(ForceUpdate)──→
    ///                    │
    ///                    ↓
    ///                 "done"  (all assemblies recompiled, all errors collected)
    ///
    /// State file: Temp/UnityMcpCompileState.json
    /// Read by Python side across domain reloads.
    /// </summary>
    public static class UnityCommandHandler
    {
        // ── In-memory tracking (optimization; state file is authority) ──
        private static bool _compileInProgress;
        private static double _compileStartTime;
        private static bool _compilationWasRunning;
        private static bool _recoveredFromReload;
        private static bool _pendingFullRebuild;

        // ── State file phases ─────────────────────────────────────────
        private const string PhaseCompiling = "compiling";
        private const string PhaseDone = "done";
        private const string PhaseNoChanges = "no_changes";

        // ── Grace period before declaring "no_changes" ────────────────
        private const double NoChangesGracePeriod = 2.0;

        // ── Sentinel for full rebuild ─────────────────────────────────
        private const string SentinelPath =
            "Assets/Scripts/Editor/UnityMcp/_CompileSentinel.cs";

        /// <summary>
        /// True when a full rebuild is pending — UnityMcpBridge calls
        /// TriggerFullRebuild() after sending the TCP response.
        /// </summary>
        public static bool HasPendingFullRebuild => _pendingFullRebuild;

        private static string StateFilePath =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath),
                "Temp", "UnityMcpCompileState.json");

        // ── Static registration ───────────────────────────────────────

        static UnityCommandHandler()
        {
            CommandHandlerRegistry.Register("ping", Ping);
            CommandHandlerRegistry.Register("list_hierarchy", ListHierarchy);
            CommandHandlerRegistry.Register("compile", Compile);
            CommandHandlerRegistry.Register("compile_status", CompileStatus);
            CommandHandlerRegistry.Register("cleanup_compile_state",
                CleanupCompileState);
            CommandHandlerRegistry.Register("refresh_assets",
                RefreshAssets);
            CommandHandlerRegistry.Register("import_asset",
                ImportAsset);
            CommandHandlerRegistry.Register("create_prefab",
                CreatePrefab);
            CommandHandlerRegistry.Register("add_gameobject",
                AddGameObject);
            CommandHandlerRegistry.Register("remove_gameobject",
                RemoveGameObject);
            CommandHandlerRegistry.Register("delete_prefab",
                DeletePrefab);
            CommandHandlerRegistry.Register("add_component",
                AddComponent);
            CommandHandlerRegistry.Register("remove_component",
                RemoveComponent);
            CommandHandlerRegistry.Register("set_component_property",
                SetComponentProperty);
            CommandHandlerRegistry.Register("generate_ui_code",
                GenerateUiCode);
        }

        /// <summary>Public entry point for UnityMcpBridge.</summary>
        public static CmdResult Execute(
            string method, Dictionary<string, string> args)
        {
            return CommandHandlerRegistry.Execute(method, args);
        }

        // ═══════════════════════════════════════════════════════════════
        //  ping
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult Ping(Dictionary<string, string> args)
        {
            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "ok",
                ["unityVersion"] = Application.unityVersion,
                ["projectName"] = Application.productName,
            });
        }

        // ═══════════════════════════════════════════════════════════════
        //  compile
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Start compilation. Returns false if a compilation is already
        /// in progress (caller should wait/retry).
        ///
        /// Called from both the MCP compile handler and the file-IPC
        /// DevToolFileTrigger.
        /// </summary>
        public static bool StartCompile(bool fullRebuild)
        {
            if (EditorApplication.isCompiling)
                return false;

            // Clean up orphan scripts BEFORE compilation starts.
            // Files deleted outside Unity leave stale .meta and .csproj
            // references that cause CS2001 errors if not cleaned first.
            UnityMcpBridge.SyncOrphanScriptsBeforeCompile();

            // Wipe any stale state from a previous run.
            DeleteStateFile();

            _compileInProgress = true;
            _compileStartTime = EditorApplication.timeSinceStartup;
            _compilationWasRunning = false;
            _recoveredFromReload = false;

            WriteStateFile(new CompileStateData
            {
                phase = PhaseCompiling,
                startTime = _compileStartTime,
                fullRebuild = fullRebuild,
                errors = new List<CompileMsgEntry>(),
                warnings = new List<CompileMsgEntry>(),
            });

            if (fullRebuild)
            {
                // Full rebuild: touch sentinel to guarantee Unity sees a
                // script delta. AssetDatabase.Refresh is deferred — the
                // MCP path triggers it via TriggerFullRebuild() after the
                // TCP response; the file-IPC path triggers it inline
                // (no TCP response to race with).
                TouchSentinel();
                _pendingFullRebuild = true;
            }
            else
            {
                // Incremental: let Unity decide whether scripts changed.
                // No domain reload if nothing changed.
                CompilationPipeline.RequestScriptCompilation();
            }

            return true;
        }

        /// <summary>
        /// Public read access to the compile state file. Returns null if
        /// the file doesn't exist or can't be parsed.
        /// </summary>
        public static CompileStateData GetCompileState()
        {
            return ReadStateFile();
        }

        private static CmdResult Compile(Dictionary<string, string> args)
        {
            // Parse fullRebuild flag (default: false = incremental).
            args.TryGetValue("fullRebuild", out var fullRebuildStr);
            bool fullRebuild = fullRebuildStr == "true";

            if (!StartCompile(fullRebuild))
            {
                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "already_compiling",
                    ["message"] = "A compilation is already in progress.",
                });
            }

            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "compilation_started",
                ["message"] = fullRebuild
                    ? "Full rebuild triggered (sentinel touch + ForceUpdate)."
                    : "Incremental compilation requested.",
            });
        }

        /// <summary>
        /// Called by UnityMcpBridge after the compile TCP response has
        /// been sent. Executes the deferred AssetDatabase.Refresh for
        /// full rebuilds. Must run on the main thread.
        /// </summary>
        public static void TriggerFullRebuild()
        {
            if (!_pendingFullRebuild) return;
            _pendingFullRebuild = false;
#if UNITY_MCP_VERBOSE
            Debug.Log("[UnityMcp] Triggering full rebuild "
                      + "(AssetDatabase.Refresh ForceUpdate)");
#endif
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        // ═══════════════════════════════════════════════════════════════
        //  Helpers (continued)
        // ═══════════════════════════════════════════════════════════════

        private static void TouchSentinel()
        {
            try
            {
                var path = Path.Combine(
                    Path.GetDirectoryName(Application.dataPath),
                    SentinelPath);
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path,
                    $"// Compile sentinel — touched {DateTime.Now:O}\n"
                    + "// DO NOT EDIT.\n");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[UnityMcp] Failed to touch sentinel: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  compile_status
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult CompileStatus(
            Dictionary<string, string> args)
        {
            var state = ReadStateFile();

            // Terminal states — return results from file.
            if (state != null &&
                (state.phase == PhaseDone ||
                 state.phase == PhaseNoChanges))
            {
                return BuildStatusResult(state);
            }

            // Still in progress or no state file at all.
            bool isCompiling = EditorApplication.isCompiling;
            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["isCompiling"] = isCompiling,
                ["compileFinished"] = false,
                ["noChanges"] = false,
                ["errorCount"] = state?.errors?.Count ?? 0,
                ["warningCount"] = state?.warnings?.Count ?? 0,
            });
        }

        // ═══════════════════════════════════════════════════════════════
        //  cleanup_compile_state
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult CleanupCompileState(
            Dictionary<string, string> args)
        {
            DeleteStateFile();
            _compileInProgress = false;
            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "cleaned",
            });
        }

        // ═══════════════════════════════════════════════════════════════
        //  refresh_assets
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult RefreshAssets(
            Dictionary<string, string> args)
        {
            AssetDatabase.Refresh();
            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "refreshed",
            });
        }

        // ═══════════════════════════════════════════════════════════════
        //  import_asset — explicitly import a single asset
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult ImportAsset(
            Dictionary<string, string> args)
        {
            args.TryGetValue("path", out var assetPath);
            if (string.IsNullOrEmpty(assetPath))
                return CmdResult.Err("Missing parameter: path");

            AssetDatabase.ImportAsset(assetPath,
                ImportAssetOptions.ForceUpdate);
            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "imported",
                ["path"] = assetPath,
            });
        }

        // ═══════════════════════════════════════════════════════════════
        //  create_prefab — programmatic prefab creation
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult CreatePrefab(
            Dictionary<string, string> args)
        {
            args.TryGetValue("path", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: path");

            if (!prefabPath.StartsWith("Assets/",
                    StringComparison.Ordinal))
                return CmdResult.Err(
                    "path must be under Assets/");

            if (!prefabPath.EndsWith(".prefab",
                    StringComparison.OrdinalIgnoreCase))
                return CmdResult.Err(
                    "path must end with .prefab");

            // viewType: "Form" (default) or "Part".
            args.TryGetValue("viewType", out var viewTypeStr);
            var isPart = viewTypeStr == "Part";

            // Only relevant for Form.
            args.TryGetValue("canvasType", out var canvasTypeStr);
            var renderMode = RenderMode.ScreenSpaceOverlay;
            if (!string.IsNullOrEmpty(canvasTypeStr))
            {
                switch (canvasTypeStr)
                {
                    case "ScreenSpaceCamera":
                        renderMode = RenderMode.ScreenSpaceCamera;
                        break;
                    case "WorldSpace":
                        renderMode = RenderMode.WorldSpace;
                        break;
                }
            }

            // formType: only for Form.
            args.TryGetValue("formType", out var formTypeStr);
            if (string.IsNullOrEmpty(formTypeStr))
                formTypeStr = "Popup";

            try
            {
                var dir = Path.GetDirectoryName(prefabPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var prefabName = Path.GetFileNameWithoutExtension(
                    prefabPath);

                GameObject go;
                var components = new List<string>();

                if (isPart)
                {
                    // ── Part: RectTransform + UiViewBinding only ──
                    // No Canvas/CanvasScaler/GraphicRaycaster/UIFormScript.
                    // Unity auto-adds a temporary parent Canvas in prefab
                    // view; it is NOT part of the prefab.
                    go = new GameObject(prefabName,
                        typeof(RectTransform));

                    var binding = go.AddComponent<UiViewBinding>();
                    binding.viewType = UiViewBinding.ViewType.Part;
                    binding.formName = "";
                    binding.namespaceName = "GenBall.UI";
                    components.Add("UiViewBinding");
                }
                else
                {
                    // ── Form: full Canvas + UIFormScript stack ──
                    go = new GameObject(prefabName);

                    var canvas = go.AddComponent<Canvas>();
                    canvas.renderMode = renderMode;
                    components.Add("Canvas");

                    // ScreenSpace only.
                    if (renderMode != RenderMode.WorldSpace)
                    {
                        var scaler =
                            go.AddComponent<CanvasScaler>();
                        scaler.uiScaleMode = CanvasScaler
                            .ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution =
                            new Vector2(1920, 1080);
                        scaler.screenMatchMode = CanvasScaler
                            .ScreenMatchMode
                            .MatchWidthOrHeight;
                        scaler.matchWidthOrHeight = 0.5f;
                        components.Add("CanvasScaler");

                        go.AddComponent<GraphicRaycaster>();
                        components.Add("GraphicRaycaster");
                    }

                    go.AddComponent<UIFormScript>();
                    components.Add("UIFormScript");

                    var binding =
                        go.AddComponent<UiViewBinding>();
                    binding.viewType = UiViewBinding.ViewType.Form;
                    binding.formType = ParseFormType(formTypeStr);
                    binding.formName = "";
                    binding.namespaceName = "GenBall.UI";
                    components.Add("UiViewBinding");
                }

                // Save.
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                UnityEngine.Object.DestroyImmediate(go);

                var prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(
                        prefabPath);
                if (prefab == null)
                    return CmdResult.Err(
                        $"Created but failed to load: {prefabPath}");

                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "created",
                    ["prefabPath"] = prefabPath,
                    ["prefabName"] = prefabName,
                    ["viewType"] = isPart ? "Part" : "Form",
                    ["canvasType"] = isPart
                        ? ""
                        : renderMode.ToString(),
                    ["hierarchy"] = BuildTree(prefab.transform),
                    ["components"] = components,
                });
            }
            catch (Exception ex)
            {
                return CmdResult.Err(
                    $"Failed to create prefab: {ex.Message}");
            }
        }

        private static UiViewBinding.FormTypeEnum ParseFormType(
            string formType)
        {
            switch (formType)
            {
                case "Persistent":
                    return UiViewBinding.FormTypeEnum.Persistent;
                case "Transition":
                    return UiViewBinding.FormTypeEnum.Transition;
                case "WorldSpace":
                    return UiViewBinding.FormTypeEnum.WorldSpace;
                default:
                    return UiViewBinding.FormTypeEnum.Popup;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  add_gameobject — programmatic child control creation
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Prefix → component type full names. Mirrors
        /// UiViewBindingEditor.DefaultMappings. Longest-prefix-first
        /// to resolve Scrollbar vs Scroll conflicts.
        /// Compounds: Btn adds Button+Image, Input adds InputField+Image,
        /// Scrollbar adds Scrollbar+Image. All others single-component.
        /// Rect prefix adds no extra component (RectTransform always exists).
        /// </summary>
        private static readonly (string prefix, string[] typeNames)[]
            PrefixComponents =
            {
                ("CanvasGroup", new[] { "UnityEngine.CanvasGroup" }),
                ("LayoutElem", new[] { "UnityEngine.UI.LayoutElement" }),
                ("Scrollbar", new[] { "UnityEngine.UI.Scrollbar", "UnityEngine.UI.Image" }),
                ("Dropdown", new[] { "UnityEngine.UI.Dropdown" }),
                ("HLayout", new[] { "UnityEngine.UI.HorizontalLayoutGroup" }),
                ("VLayout", new[] { "UnityEngine.UI.VerticalLayoutGroup" }),
                ("RawImg", new[] { "UnityEngine.UI.RawImage" }),
                ("Scroll", new[] { "UnityEngine.UI.ScrollRect" }),
                ("Slider", new[] { "UnityEngine.UI.Slider" }),
                ("Toggle", new[] { "UnityEngine.UI.Toggle" }),
                ("Fitter", new[] { "UnityEngine.UI.ContentSizeFitter" }),
                ("Input", new[] { "UnityEngine.UI.InputField", "UnityEngine.UI.Image" }),
                ("Grid", new[] { "UnityEngine.UI.GridLayoutGroup" }),
                ("Rect", new string[] { }),
                ("Btn", new[] { "UnityEngine.UI.Button", "UnityEngine.UI.Image" }),
                ("Img", new[] { "UnityEngine.UI.Image" }),
                ("Txt", new[] { "UnityEngine.UI.Text" }),
            };

        private static CmdResult AddGameObject(
            Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            args.TryGetValue("name", out var name);
            if (string.IsNullOrEmpty(name))
                return CmdResult.Err("Missing parameter: name");

            args.TryGetValue("parentPath", out var parentPath);

            // Validate prefab exists.
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    == null)
                return CmdResult.Err(
                    $"Prefab not found at: {prefabPath}");

            try
            {
                var contents =
                    PrefabUtility.LoadPrefabContents(prefabPath);

                // Locate parent.
                Transform parent;
                if (string.IsNullOrEmpty(parentPath))
                {
                    parent = contents.transform;
                }
                else
                {
                    parent = contents.transform.Find(parentPath);
                    if (parent == null)
                    {
                        PrefabUtility.UnloadPrefabContents(contents);
                        return CmdResult.Err(
                            $"Parent not found: {parentPath}");
                    }
                }

                // Create child with RectTransform (UI context).
                var child = new GameObject(name, typeof(RectTransform));
                child.transform.SetParent(parent, false);

                // Prefix → component matching.
                var added = AddComponentsByPrefix(child, name);

                // Save.
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                PrefabUtility.UnloadPrefabContents(contents);

                // Build child path.
                var childPath = string.IsNullOrEmpty(parentPath)
                    ? name
                    : parentPath + "/" + name;

                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "added",
                    ["prefabPath"] = prefabPath,
                    ["gameObject"] = name,
                    ["path"] = childPath,
                    ["components"] = added,
                });
            }
            catch (Exception ex)
            {
                return CmdResult.Err(
                    $"Failed to add game object: {ex.Message}");
            }
        }

        /// <summary>
        /// Match a GameObject name against prefix conventions and add
        /// the corresponding Unity components. Returns the short type
        /// names of all added components.
        /// </summary>
        private static List<string> AddComponentsByPrefix(
            GameObject go, string name)
        {
            var added = new List<string>();

            // Longest-prefix-first (resolves Scrollbar vs Scroll).
            foreach (var (prefix, typeNames) in PrefixComponents)
            {
                if (!name.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                foreach (var fullName in typeNames)
                {
                    var t = FindType(fullName);
                    if (t == null) continue;

                    go.AddComponent(t);

                    // Short display name (last segment).
                    var shortName = fullName.Substring(
                        fullName.LastIndexOf('.') + 1);
                    added.Add(shortName);
                }
                break; // first (longest) match wins
            }

            // If no prefix matched, still have an empty list — caller
            // can see that only RectTransform exists.
            return added;
        }

        // ═══════════════════════════════════════════════════════════════
        //  delete_prefab — delete a .prefab asset (with .meta)
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult DeletePrefab(
            Dictionary<string, string> args)
        {
            args.TryGetValue("path", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: path");

            if (!prefabPath.EndsWith(".prefab",
                    StringComparison.OrdinalIgnoreCase))
                return CmdResult.Err(
                    "path must end with .prefab");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    == null)
                return CmdResult.Err(
                    $"Prefab not found at: {prefabPath}");

            try
            {
                AssetDatabase.DeleteAsset(prefabPath);

                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "deleted",
                    ["path"] = prefabPath,
                });
            }
            catch (Exception ex)
            {
                return CmdResult.Err(
                    $"Failed to delete prefab: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  remove_gameobject — delete a child from a prefab
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult RemoveGameObject(
            Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            args.TryGetValue("path", out var childPath);
            if (string.IsNullOrEmpty(childPath))
                return CmdResult.Err("Missing parameter: path");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    == null)
                return CmdResult.Err(
                    $"Prefab not found at: {prefabPath}");

            try
            {
                var contents =
                    PrefabUtility.LoadPrefabContents(prefabPath);

                var target = contents.transform.Find(childPath);
                if (target == null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        $"GameObject not found: {childPath}");
                }

                if (target == contents.transform)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        "Cannot remove the prefab root.");
                }

                UnityEngine.Object.DestroyImmediate(
                    target.gameObject);

                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                PrefabUtility.UnloadPrefabContents(contents);

                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "removed",
                    ["prefabPath"] = prefabPath,
                    ["path"] = childPath,
                });
            }
            catch (Exception ex)
            {
                return CmdResult.Err(
                    $"Failed to remove game object: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  add_component — add a component by type name
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult AddComponent(
            Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            args.TryGetValue("path", out var childPath);
            if (string.IsNullOrEmpty(childPath))
                return CmdResult.Err("Missing parameter: path");

            args.TryGetValue("componentType", out var compTypeName);
            if (string.IsNullOrEmpty(compTypeName))
                return CmdResult.Err(
                    "Missing parameter: componentType");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    == null)
                return CmdResult.Err(
                    $"Prefab not found at: {prefabPath}");

            try
            {
                var contents =
                    PrefabUtility.LoadPrefabContents(prefabPath);

                var target = contents.transform.Find(childPath);
                if (target == null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        $"GameObject not found: {childPath}");
                }

                // Try full name first, then common namespace variants.
                var t = FindType(compTypeName)
                    ?? FindType(compTypeName + ", UnityEngine.UI")
                    ?? FindType(compTypeName + ", UnityEngine");

                if (t == null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        $"Unknown component type: {compTypeName}");
                }

                target.gameObject.AddComponent(t);

                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                PrefabUtility.UnloadPrefabContents(contents);

                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "added",
                    ["prefabPath"] = prefabPath,
                    ["path"] = childPath,
                    ["component"] = t.Name,
                });
            }
            catch (Exception ex)
            {
                return CmdResult.Err(
                    $"Failed to add component: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  remove_component — remove a component by type name
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult RemoveComponent(
            Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            args.TryGetValue("path", out var childPath);
            if (string.IsNullOrEmpty(childPath))
                return CmdResult.Err("Missing parameter: path");

            args.TryGetValue("componentType", out var compTypeName);
            if (string.IsNullOrEmpty(compTypeName))
                return CmdResult.Err(
                    "Missing parameter: componentType");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    == null)
                return CmdResult.Err(
                    $"Prefab not found at: {prefabPath}");

            try
            {
                var contents =
                    PrefabUtility.LoadPrefabContents(prefabPath);

                var target = contents.transform.Find(childPath);
                if (target == null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        $"GameObject not found: {childPath}");
                }

                // Short name matching (e.g. "Image", "Button").
                Component found = null;
                foreach (var c in target.GetComponents<Component>())
                {
                    if (c == null) continue;
                    var tn = c.GetType().Name;
                    var fn = c.GetType().FullName;
                    if (tn == compTypeName || fn == compTypeName)
                    {
                        found = c;
                        break;
                    }
                }

                if (found == null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        $"Component not found: {compTypeName}"
                        + $" on {childPath}");
                }

                // Prevent removing mandatory components.
                var mandatory = new HashSet<string>
                    { "Transform", "RectTransform" };
                if (mandatory.Contains(found.GetType().Name))
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        $"Cannot remove mandatory component: "
                        + found.GetType().Name);
                }

                UnityEngine.Object.DestroyImmediate(found);

                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                PrefabUtility.UnloadPrefabContents(contents);

                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "removed",
                    ["prefabPath"] = prefabPath,
                    ["path"] = childPath,
                    ["component"] = found.GetType().Name,
                });
            }
            catch (Exception ex)
            {
                return CmdResult.Err(
                    $"Failed to remove component: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  set_component_property — set a serialized property value
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult SetComponentProperty(
            Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            args.TryGetValue("path", out var childPath);
            if (string.IsNullOrEmpty(childPath))
                return CmdResult.Err("Missing parameter: path");

            args.TryGetValue("componentType", out var compTypeName);
            if (string.IsNullOrEmpty(compTypeName))
                return CmdResult.Err(
                    "Missing parameter: componentType");

            args.TryGetValue("property", out var propName);
            if (string.IsNullOrEmpty(propName))
                return CmdResult.Err("Missing parameter: property");

            args.TryGetValue("value", out var valueStr);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
                    == null)
                return CmdResult.Err(
                    $"Prefab not found at: {prefabPath}");

            try
            {
                var contents =
                    PrefabUtility.LoadPrefabContents(prefabPath);

                var target = contents.transform.Find(childPath);
                if (target == null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        $"GameObject not found: {childPath}");
                }

                Component comp = null;
                foreach (var c in target.GetComponents<Component>())
                {
                    if (c == null) continue;
                    var tn = c.GetType().Name;
                    var fn = c.GetType().FullName;
                    if (tn == compTypeName || fn == compTypeName)
                    {
                        comp = c;
                        break;
                    }
                }

                if (comp == null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                    return CmdResult.Err(
                        $"Component not found: {compTypeName}"
                        + $" on {childPath}");
                }

                using (var so = new SerializedObject(comp))
                {
                    var sp = so.FindProperty(propName);
                    if (sp == null)
                    {
                        PrefabUtility.UnloadPrefabContents(contents);
                        return CmdResult.Err(
                            $"Property not found: {propName} on "
                            + compTypeName);
                    }

                    var ok = TrySetSerializedProperty(
                        sp, valueStr);
                    if (!ok)
                    {
                        PrefabUtility.UnloadPrefabContents(contents);
                        return CmdResult.Err(
                            $"Failed to set {propName} to "
                            + $"'{valueStr}' (type={sp.propertyType})");
                    }

                    so.ApplyModifiedProperties();
                }

                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                PrefabUtility.UnloadPrefabContents(contents);

                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "property_set",
                    ["prefabPath"] = prefabPath,
                    ["path"] = childPath,
                    ["component"] = comp.GetType().Name,
                    ["property"] = propName,
                    ["value"] = valueStr ?? "",
                });
            }
            catch (Exception ex)
            {
                return CmdResult.Err(
                    $"Failed to set property: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse a string value into the appropriate SerializedProperty
        /// type. Supports integer, float, bool, string, Vector2/3/4,
        /// Color, Rect, and enum (by name or int).
        /// </summary>
        private static bool TrySetSerializedProperty(
            SerializedProperty sp, string value)
        {
            switch (sp.propertyType)
            {
                case SerializedPropertyType.Integer:
                    if (int.TryParse(value, out var iv))
                    { sp.intValue = iv; return true; }
                    return false;

                case SerializedPropertyType.Float:
                    if (float.TryParse(value,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo
                                .InvariantCulture,
                            out var fv))
                    { sp.floatValue = fv; return true; }
                    return false;

                case SerializedPropertyType.Boolean:
                    if (bool.TryParse(value, out var bv))
                    { sp.boolValue = bv; return true; }
                    return false;

                case SerializedPropertyType.String:
                    sp.stringValue = value ?? "";
                    return true;

                case SerializedPropertyType.Color:
                    if (ColorUtility.TryParseHtmlString(
                            value, out var col))
                    { sp.colorValue = col; return true; }
                    return false;

                case SerializedPropertyType.Vector2:
                    var v2 = ParseVector2(value);
                    if (v2.HasValue)
                    { sp.vector2Value = v2.Value; return true; }
                    return false;

                case SerializedPropertyType.Vector3:
                    var v3 = ParseVector3(value);
                    if (v3.HasValue)
                    { sp.vector3Value = v3.Value; return true; }
                    return false;

                case SerializedPropertyType.Vector4:
                    var v4 = ParseVector4(value);
                    if (v4.HasValue)
                    { sp.vector4Value = v4.Value; return true; }
                    return false;

                case SerializedPropertyType.Rect:
                    var r = ParseRect(value);
                    if (r.HasValue)
                    { sp.rectValue = r.Value; return true; }
                    return false;

                case SerializedPropertyType.Enum:
                    // Try integer index first, then name.
                    if (int.TryParse(value, out var ei))
                    { sp.enumValueIndex = ei; return true; }
                    return false;

                case SerializedPropertyType.ObjectReference:
                    if (string.IsNullOrEmpty(value)
                            || value == "null")
                    {
                        sp.objectReferenceValue = null;
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        private static Vector2? ParseVector2(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var parts = value.Trim('(', ')', ' ')
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2
                && float.TryParse(parts[0],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var x)
                && float.TryParse(parts[1],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var y))
                return new Vector2(x, y);
            return null;
        }

        private static Vector3? ParseVector3(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var parts = value.Trim('(', ')', ' ')
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3
                && float.TryParse(parts[0],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var x)
                && float.TryParse(parts[1],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var y)
                && float.TryParse(parts[2],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var z))
                return new Vector3(x, y, z);
            return null;
        }

        private static Vector4? ParseVector4(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var parts = value.Trim('(', ')', ' ')
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4
                && float.TryParse(parts[0],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var x)
                && float.TryParse(parts[1],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var y)
                && float.TryParse(parts[2],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var z)
                && float.TryParse(parts[3],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var w))
                return new Vector4(x, y, z, w);
            return null;
        }

        private static Rect? ParseRect(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var parts = value.Trim('(', ')', ' ')
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4
                && float.TryParse(parts[0],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var x)
                && float.TryParse(parts[1],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var y)
                && float.TryParse(parts[2],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var w)
                && float.TryParse(parts[3],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo
                        .InvariantCulture,
                    out var h))
                return new Rect(x, y, w, h);
            return null;
        }

        // ═══════════════════════════════════════════════════════════════
        //  generate_ui_code — scan prefab + generate View/Logic/ViewData
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult GenerateUiCode(
            Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            var prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return CmdResult.Err(
                    $"Prefab not found at: {prefabPath}");

            args.TryGetValue("formName", out var formName);
            if (string.IsNullOrEmpty(formName))
                formName = Path.GetFileNameWithoutExtension(
                    prefabPath);

            args.TryGetValue("viewType", out var viewTypeStr);
            var viewType = viewTypeStr == "Part"
                ? UiViewBinding.ViewType.Part
                : UiViewBinding.ViewType.Form;

            args.TryGetValue("formType", out var formType);
            if (string.IsNullOrEmpty(formType))
                formType = "Popup";

            args.TryGetValue("namespace", out var ns);
            if (string.IsNullOrEmpty(ns))
                ns = "GenBall.UI";

            args.TryGetValue("outputPath", out var outputDir);
            if (string.IsNullOrEmpty(outputDir))
                outputDir =
                    $"Assets/Scripts/GenBall/UI/{formName}";

            try
            {
                // 1. Get or build config (fallback to defaults).
                var config = GetOrCreateBindingConfig();

                // 2. Scan the prefab.
                var scanResult = UiPrefabScanner.Scan(
                    prefab, config, prefabPath);
                scanResult.formName = formName;

                // 3. Determine base classes.
                string viewBase, logicBase;
                if (viewType == UiViewBinding.ViewType.Part)
                {
                    viewBase = "Yueyn.UI.PartViewBase";
                    logicBase = "Yueyn.UI.BusinessPartLogic";
                }
                else
                {
                    viewBase = "Yueyn.UI.UIBusinessFormBase";
                    logicBase = "Yueyn.UI.BusinessFormLogic";
                }

                // 4. Generate code files.
                UiBindingCodeGenerator.GenerateAll(
                    scanResult, formName, formType,
                    outputDir, ns,
                    viewBase, logicBase, viewType);

                // 5. Collect generated files.
                var files = new List<string>();
                var viewFile = Path.Combine(outputDir,
                    UiBindingCodeGenerator.GetViewFileName(
                        formName));
                var logicFile = Path.Combine(outputDir,
                    UiBindingCodeGenerator.GetLogicFileName(
                        formName));
                var viewDataFile = Path.Combine(outputDir,
                    UiBindingCodeGenerator.GetViewDataFileName(
                        formName));
                if (File.Exists(viewFile))
                    files.Add(viewFile.Replace("\\", "/"));
                if (File.Exists(logicFile))
                    files.Add(logicFile.Replace("\\", "/"));
                if (File.Exists(viewDataFile))
                    files.Add(viewDataFile.Replace("\\", "/"));

                return CmdResult.Ok(
                    new Dictionary<string, object>
                    {
                        ["status"] = "generated",
                        ["prefabPath"] = prefabPath,
                        ["formName"] = formName,
                        ["viewType"] = viewTypeStr,
                        ["formType"] = formType,
                        ["outputDir"] = outputDir,
                        ["files"] = files,
                        ["bindingCount"] =
                            scanResult.bindings.Count,
                        ["warnings"] = scanResult.warnings,
                    });
            }
            catch (Exception ex)
            {
                return CmdResult.Err(
                    $"Failed to generate code: {ex.Message}");
            }
        }

        /// <summary>
        /// Get or build a UiBindingConfig. Tries to find one in the
        /// project first; falls back to a temporary config built from
        /// the hardcoded default 17 mappings.
        /// </summary>
        private static UiBindingConfig GetOrCreateBindingConfig()
        {
            // Try project first.
            var guids = AssetDatabase.FindAssets(
                "t:UiBindingConfig");
            if (guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<
                    UiBindingConfig>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));

            // Fallback: build from DefaultMappings.
            var config = ScriptableObject
                .CreateInstance<UiBindingConfig>();

            var defaults = new (string, string, string, string,
                string, int)[]
            {
                ("Btn", "Button",
                    "UnityEngine.UI.Button", "UnityEngine.UI",
                    "interactive", 10),
                ("Txt", "Text",
                    "UnityEngine.UI.Text", "UnityEngine.UI",
                    "display", 20),
                ("Img", "Image",
                    "UnityEngine.UI.Image", "UnityEngine.UI",
                    "display", 30),
                ("RawImg", "RawImage",
                    "UnityEngine.UI.RawImage", "UnityEngine.UI",
                    "display", 35),
                ("Rect", "RectTransform",
                    "UnityEngine.RectTransform", "UnityEngine",
                    "layout", 40),
                ("Input", "InputField",
                    "UnityEngine.UI.InputField", "UnityEngine.UI",
                    "interactive", 50),
                ("Slider", "Slider",
                    "UnityEngine.UI.Slider", "UnityEngine.UI",
                    "interactive", 60),
                ("Toggle", "Toggle",
                    "UnityEngine.UI.Toggle", "UnityEngine.UI",
                    "interactive", 70),
                ("Scroll", "ScrollRect",
                    "UnityEngine.UI.ScrollRect", "UnityEngine.UI",
                    "interactive", 80),
                ("Dropdown", "Dropdown",
                    "UnityEngine.UI.Dropdown", "UnityEngine.UI",
                    "interactive", 90),
                ("Scrollbar", "Scrollbar",
                    "UnityEngine.UI.Scrollbar", "UnityEngine.UI",
                    "interactive", 100),
                ("CanvasGroup", "CanvasGroup",
                    "UnityEngine.CanvasGroup", "UnityEngine",
                    "layout", 110),
                ("LayoutElem", "LayoutElement",
                    "UnityEngine.UI.LayoutElement",
                    "UnityEngine.UI", "layout", 120),
                ("Fitter", "ContentSizeFitter",
                    "UnityEngine.UI.ContentSizeFitter",
                    "UnityEngine.UI", "layout", 130),
                ("HLayout", "HorizontalLayoutGroup",
                    "UnityEngine.UI.HorizontalLayoutGroup",
                    "UnityEngine.UI", "layout", 140),
                ("VLayout", "VerticalLayoutGroup",
                    "UnityEngine.UI.VerticalLayoutGroup",
                    "UnityEngine.UI", "layout", 150),
                ("Grid", "GridLayoutGroup",
                    "UnityEngine.UI.GridLayoutGroup",
                    "UnityEngine.UI", "layout", 160),
            };

            foreach (var (prefix, compType, fullName,
                         useNs, cat, pri) in defaults)
            {
                config.prefixMappings.Add(new PrefixMapping
                {
                    prefix = prefix,
                    componentType = compType,
                    fullName = fullName,
                    usingNamespace = useNs,
                    category = cat,
                    priority = pri,
                });
            }

            return config;
        }

        // ═══════════════════════════════════════════════════════════════
        //  CollectCompileMessages  (called by UnityMcpBridge each frame)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Called by CompilationPipeline.assemblyCompilationFinished.
        /// Appends errors/warnings to the state file so they survive
        /// the domain reload that follows compilation.
        /// </summary>
        public static void CollectCompileMessages(
            string assemblyPath, CompilerMessage[] messages)
        {
            if (!_compileInProgress) return;
            if (messages == null || messages.Length == 0) return;

            var state = ReadStateFile();
            if (state == null) return;

            foreach (var msg in messages)
            {
                var entry = new CompileMsgEntry
                {
                    file = msg.file ?? "",
                    line = msg.line,
                    column = msg.column,
                    message = msg.message ?? "",
                };

                if (msg.type == CompilerMessageType.Error)
                    state.errors.Add(entry);
                else if (msg.type == CompilerMessageType.Warning)
                    state.warnings.Add(entry);
            }

            WriteStateFile(state);
        }

        // ═══════════════════════════════════════════════════════════════
        //  CheckCompileCompletion  (called by UnityMcpBridge each frame)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Polled every Editor update. Detects when compilation finishes
        /// and transitions the state file to its terminal phase.
        ///
        /// Handles three scenarios:
        /// 1. <b>Normal compilation</b>: isCompiling flips true→false
        ///    within the same domain → "done".
        /// 2. <b>Domain reload</b>: the reload itself is evidence that
        ///    compilation happened → "done".
        /// 3. <b>No script changes</b>: RequestScriptCompilation returns
        ///    without starting → after grace period → "no_changes".
        /// </summary>
        public static void CheckCompileCompletion()
        {
            // ── Recovery from domain reload ──
            if (!_compileInProgress)
            {
                var recovered = ReadStateFile();
                if (recovered != null && recovered.phase == PhaseCompiling)
                {
                    _compileInProgress = true;
                    _compileStartTime = recovered.startTime;
                    _compilationWasRunning = false;
                    _recoveredFromReload = true;
#if UNITY_MCP_VERBOSE
                    Debug.Log("[UnityMcp] Recovered compile state after "
                              + "domain reload.");
#endif
                }
                else
                {
                    return; // Nothing in progress.
                }
            }

            // Track whether Unity actually started compiling.
            if (EditorApplication.isCompiling)
            {
                _compilationWasRunning = true;
                return;
            }

            // ── Not compiling — decide terminal phase ──

            var state = ReadStateFile();
            if (state == null) return;

            if (state.phase != PhaseCompiling) return; // already terminal

            if (_recoveredFromReload)
            {
                // We just recovered from a domain reload → compilation
                // must have happened (the reload proves it).
                state.phase = PhaseDone;
                _recoveredFromReload = false;
#if UNITY_MCP_VERBOSE
                Debug.Log(
                    $"[UnityMcp] Compile done (post-reload). "
                    + $"Errors={state.errors.Count} "
                    + $"Warnings={state.warnings.Count}");
#endif
            }
            else if (_compilationWasRunning)
            {
                // Compilation ran and finished within this domain.
                state.phase = PhaseDone;
#if UNITY_MCP_VERBOSE
                Debug.Log(
                    $"[UnityMcp] Compile done. "
                    + $"Errors={state.errors.Count} "
                    + $"Warnings={state.warnings.Count}");
#endif
            }
            else
            {
                // Compilation never started. Wait a grace period to
                // be sure Unity isn't just about to start.
                double elapsed = EditorApplication.timeSinceStartup
                    - _compileStartTime;
                if (elapsed < NoChangesGracePeriod)
                    return;

                state.phase = PhaseNoChanges;
#if UNITY_MCP_VERBOSE
                Debug.Log("[UnityMcp] No script changes detected "
                          + "— compile skipped.");
#endif
            }

            WriteStateFile(state);
            _compileInProgress = false;
        }

        // ═══════════════════════════════════════════════════════════════
        //  list_hierarchy
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult ListHierarchy(
            Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                prefabPath);
            if (prefab == null)
                return CmdResult.Err($"Prefab not found at: {prefabPath}");

            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["prefabPath"] = prefabPath,
                ["hierarchy"] = BuildTree(prefab.transform),
                ["totalObjects"] = CountAll(prefab.transform),
            });
        }

        // ═══════════════════════════════════════════════════════════════
        //  State file I/O
        // ═══════════════════════════════════════════════════════════════

        private static CompileStateData ReadStateFile()
        {
            try
            {
                if (!File.Exists(StateFilePath)) return null;
                var json = File.ReadAllText(StateFilePath);
                return JsonUtility.FromJson<CompileStateData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[UnityMcp] Failed to read compile state: {ex.Message}");
                return null;
            }
        }

        private static void WriteStateFile(CompileStateData data)
        {
            try
            {
                var dir = Path.GetDirectoryName(StateFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(StateFilePath,
                    JsonUtility.ToJson(data));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[UnityMcp] Failed to write compile state: {ex.Message}");
            }
        }

        private static void DeleteStateFile()
        {
            try
            {
                if (File.Exists(StateFilePath))
                    File.Delete(StateFilePath);
            }
            catch { /* best-effort */ }
        }

        // ═══════════════════════════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult BuildStatusResult(CompileStateData state)
        {
            var errors = state.errors?.Select(e =>
                new Dictionary<string, object>
                {
                    ["file"] = e.file ?? "",
                    ["line"] = e.line,
                    ["column"] = e.column,
                    ["message"] = e.message ?? "",
                }).ToList()
                ?? new List<Dictionary<string, object>>();

            var warnings = state.warnings?.Select(w =>
                new Dictionary<string, object>
                {
                    ["file"] = w.file ?? "",
                    ["line"] = w.line,
                    ["column"] = w.column,
                    ["message"] = w.message ?? "",
                }).ToList()
                ?? new List<Dictionary<string, object>>();

            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["isCompiling"] = false,
                ["compileFinished"] = true,
                ["noChanges"] = state.phase == PhaseNoChanges,
                ["errorCount"] = errors.Count,
                ["warningCount"] = warnings.Count,
                ["errors"] = errors,
                ["warnings"] = warnings,
            });
        }

        private static Dictionary<string, object> BuildTree(Transform t)
        {
            var node = new Dictionary<string, object>
            {
                ["name"] = t.gameObject.name,
                ["active"] = t.gameObject.activeSelf,
            };

            var comps = t.GetComponents<Component>();
            var names = new List<string>();
            foreach (var c in comps)
                if (c != null) names.Add(c.GetType().Name);
            node["components"] = names;

            if (t.childCount > 0)
            {
                var children = new List<Dictionary<string, object>>();
                for (int i = 0; i < t.childCount; i++)
                    children.Add(BuildTree(t.GetChild(i)));
                node["children"] = children;
            }

            return node;
        }

        private static int CountAll(Transform t)
        {
            int n = 1;
            for (int i = 0; i < t.childCount; i++)
                n += CountAll(t.GetChild(i));
            return n;
        }

        /// <summary>
        /// Resolve a type name (full or short) to a System.Type.
        /// Examples: "UnityEngine.UI.Button", "Button", "Text",
        /// "ContentSizeFitter", "LayoutElement".
        /// </summary>
        private static Type FindType(string name)
        {
            // Try as a full name via Type.GetType with common assemblies.
            var type = Type.GetType($"{name}, UnityEngine.UI");
            if (type != null) return type;

            type = Type.GetType($"{name}, UnityEngine");
            if (type != null) return type;

            type = Type.GetType(name);
            if (type != null) return type;

            // Try appending Unity namespaces.
            foreach (var ns in new[]
                     {
                         "UnityEngine.UI",
                         "UnityEngine",
                         "UnityEditor",
                     })
            {
                type = Type.GetType($"{ns}.{name}, UnityEngine.UI");
                if (type != null) return type;
                type = Type.GetType($"{ns}.{name}, UnityEngine");
                if (type != null) return type;
            }

            // Brute-force search by full name across assemblies.
            foreach (var asm in AppDomain.CurrentDomain
                         .GetAssemblies())
            {
                type = asm.GetType(name);
                if (type != null) return type;
            }

            // Brute-force search by short name (last resort).
            foreach (var asm in AppDomain.CurrentDomain
                         .GetAssemblies())
            {
                try
                {
                    foreach (var t in asm.GetTypes())
                    {
                        if (t.Name == name || t.FullName == name)
                            return t;
                    }
                }
                catch
                {
                    // Some assemblies throw on GetTypes().
                }
            }

            return null;
        }

        /// <summary>Public path for external readers (Python).</summary>
        public static string CompileStateFilePath =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath),
                "Temp", "UnityMcpCompileState.json");
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Command Handler Registry — extensible, no switch/if-else
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registry for MCP command handlers.
    ///
    /// Adding a new command only requires:
    /// 1. A handler method matching
    ///    <c>Func&lt;Dictionary&lt;string,string&gt;, CmdResult&gt;</c>
    /// 2. One <c>CommandHandlerRegistry.Register("name", handler)</c>
    ///    call
    ///
    /// No changes to dispatch logic needed.
    /// </summary>
    public static class CommandHandlerRegistry
    {
        private static readonly Dictionary<
            string, Func<Dictionary<string, string>, CmdResult>>
            _handlers = new();

        /// <summary>
        /// Register a command handler for the given method name.
        /// </summary>
        public static void Register(
            string method,
            Func<Dictionary<string, string>, CmdResult> handler)
        {
            _handlers[method] = handler;
        }

        /// <summary>
        /// Execute a registered command. Returns an error result if
        /// the method is unknown.
        /// </summary>
        public static CmdResult Execute(
            string method, Dictionary<string, string> args)
        {
            if (_handlers.TryGetValue(method, out var handler))
                return handler(args);
            return CmdResult.Err($"Unknown method: {method}");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Data types
    // ═══════════════════════════════════════════════════════════════════

    // ═══════════════════════════════════════════════════════════════════
    //  File-IPC DTOs (shared by DevToolFileTrigger logic in
    //  UnityMcpBridge)
    // ═══════════════════════════════════════════════════════════════════

    [Serializable]
    public class DevToolCompileOptions
    {
        public bool fullRebuild;
    }

    /// <summary>
    /// Result written to <c>Temp/.devtool_compile_result.json</c>.
    /// Same format as the MCP unity_compile tool output.
    /// </summary>
    [Serializable]
    public class CompileResultData
    {
        public string status;
        public int errorCount;
        public int warningCount;
        public List<CompileMsgEntry> errors;
        public List<CompileMsgEntry> warnings;
    }

    [Serializable]
    public class CompileStateData
    {
        /// <summary>compiling | done | no_changes</summary>
        public string phase;
        public double startTime;
        /// <summary>Whether this is a full rebuild (vs incremental).</summary>
        public bool fullRebuild;
        public List<CompileMsgEntry> errors;
        public List<CompileMsgEntry> warnings;
    }

    [Serializable]
    public class CompileMsgEntry
    {
        public string file;
        public int line;
        public int column;
        public string message;
    }

    /// <summary>
    /// JSON-RPC request envelope. Only extracts id and method;
    /// params are parsed separately via
    /// <c>UnityMcpBridge.ExtractParamsDict</c> so each handler can
    /// use its own strongly-typed DTO.
    /// </summary>
    [Serializable]
    public class JsonRpcEnvelope
    {
        public string id;
        public string method;
    }

    /// <summary>Params DTO for the list_hierarchy command.</summary>
    [Serializable]
    public class ListHierarchyParams
    {
        public string prefabPath;
    }

    public class CmdResult
    {
        public bool Success;
        public object Data;
        public string ErrMsg;

        public static CmdResult Ok(object data) =>
            new() { Success = true, Data = data };

        public static CmdResult Err(string msg) =>
            new() { Success = false, ErrMsg = msg };
    }
}
