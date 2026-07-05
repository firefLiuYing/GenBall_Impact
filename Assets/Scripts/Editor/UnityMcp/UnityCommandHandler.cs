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
    ///   (no file) ──Compile()──→ "refresh_pending"
    ///                                   │
    ///                         TriggerCompileRefresh()
    ///                                   │
    ///                                   ↓
    ///                              "compiling"
    ///                                   │
    ///                    ┌──────────────┴──────────────┐
    ///                    ↓                             ↓
    ///                 "done"                    "no_changes"
    ///            (errors collected)           (5s, no script delta)
    ///
    /// State file: Temp/UnityMcpCompileState.json
    /// Read by Python side across domain reloads.
    /// </summary>
    public static class UnityCommandHandler
    {
        // ── In-memory tracking (optimization; state file is authority) ──
        private static bool _compileInProgress;
        private static double _compileStartTime;

        // ── State file phases ─────────────────────────────────────────
        private const string PhaseRefreshPending = "refresh_pending";
        private const string PhaseCompiling = "compiling";
        private const string PhaseDone = "done";
        private const string PhaseNoChanges = "no_changes";

        // ── Sentinel file ─────────────────────────────────────────────
        private const string SentinelPath =
            "Assets/Scripts/Editor/UnityMcp/_CompileSentinel.cs";

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
        //  compile  (step 1: arm the trigger)
        // ═══════════════════════════════════════════════════════════════

        private static CmdResult Compile(Dictionary<string, string> args)
        {
            if (EditorApplication.isCompiling)
            {
                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "already_compiling",
                    ["message"] = "A compilation is already in progress.",
                });
            }

            // Touch sentinel so Unity always sees a script delta.
            TouchSentinel();

            // Wipe any stale state from a previous run.
            DeleteStateFile();

            _compileInProgress = true;
            _compileStartTime = EditorApplication.timeSinceStartup;

            WriteStateFile(new CompileStateData
            {
                phase = PhaseRefreshPending,
                startTime = _compileStartTime,
                sentinelTime = DateTime.Now.ToString("O"),
                errors = new List<CompileMsgEntry>(),
                warnings = new List<CompileMsgEntry>(),
            });

            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "compilation_started",
                ["message"] = "Sentinel touched. Refresh pending.",
            });
        }

        /// <summary>
        /// Called AFTER the TCP response for "compile" has been sent.
        /// Reads the state file (not in-memory flags) so it survives
        /// any domain reload that happened between Compile() and now.
        /// </summary>
        public static void TriggerCompileRefresh()
        {
            var state = ReadStateFile();
            if (state == null || state.phase != PhaseRefreshPending)
                return;

            if (EditorApplication.isCompiling)
                return; // already started by some other trigger

            state.phase = PhaseCompiling;
            WriteStateFile(state);

#if UNITY_MCP_VERBOSE
            Debug.Log(
                "[UnityMcp] Triggering AssetDatabase.Refresh (ForceUpdate)");
#endif
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
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
        /// Must handle domain reload: after reload _compileInProgress is
        /// false, but the state file still holds the real phase.
        /// </summary>
        public static void CheckCompileCompletion()
        {
            // ── Recovery from domain reload ──
            if (!_compileInProgress)
            {
                var recovered = ReadStateFile();
                if (recovered != null &&
                    (recovered.phase == PhaseRefreshPending ||
                     recovered.phase == PhaseCompiling))
                {
                    _compileInProgress = true;
                    _compileStartTime = recovered.startTime;
                }
                else
                {
                    return; // Nothing in progress.
                }
            }

            // Still compiling — wait.
            if (EditorApplication.isCompiling)
                return;

            double elapsed =
                EditorApplication.timeSinceStartup - _compileStartTime;

            var state = ReadStateFile();
            if (state == null) return;

            switch (state.phase)
            {
                case PhaseRefreshPending:
                    // TriggerCompileRefresh should have moved us to
                    // "compiling" by now. If > 5 s and still pending,
                    // the sentinel touch wasn't enough — no script delta.
                    if (elapsed > 5.0)
                    {
                        state.phase = PhaseNoChanges;
                        WriteStateFile(state);
                        _compileInProgress = false;
#if UNITY_MCP_VERBOSE
                        Debug.Log(
                            "[UnityMcp] No script changes detected "
                            + "— compile skipped.");
#endif
                    }
                    break;

                case PhaseCompiling:
                    // Compilation just finished (isCompiling → false).
                    state.phase = PhaseDone;
                    WriteStateFile(state);
                    _compileInProgress = false;
#if UNITY_MCP_VERBOSE
                    Debug.Log(
                        $"[UnityMcp] Compile done. "
                        + $"Errors={state.errors.Count} "
                        + $"Warnings={state.warnings.Count}");
#endif
                    break;

                case PhaseDone:
                case PhaseNoChanges:
                    // Already terminal — client hasn't cleaned up yet.
                    break;
            }
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
    /// 1. A handler method matching <c>Func&lt;Dictionary&lt;string,string&gt;, CmdResult&gt;</c>
    /// 2. One <c>CommandHandlerRegistry.Register("name", handler)</c> call
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

    [Serializable]
    public class CompileStateData
    {
        public string phase;
        public double startTime;
        public string sentinelTime;
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
    /// params are parsed separately via <c>ExtractParamsJson</c>
    /// so each handler can use its own strongly-typed DTO.
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
