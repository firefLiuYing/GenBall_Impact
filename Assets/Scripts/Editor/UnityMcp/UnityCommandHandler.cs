using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

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
