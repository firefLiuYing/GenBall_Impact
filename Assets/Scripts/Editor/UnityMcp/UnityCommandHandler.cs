using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Yueyn.Editor.UnityMcp
{
    public static class UnityCommandHandler
    {
        // Compilation state tracking (in-memory, lost on domain reload)
        private static readonly List<CompilerMessage> _compileErrors = new();
        private static readonly List<CompilerMessage> _compileWarnings = new();
        private static bool _compileRequested;
        private static bool _hasStartedCompiling;
        private static bool _compileFinished;
        private static double _compileRequestTime;

        // Sentinel file path — touched before each compile to ensure Unity detects changes
        private const string SentinelPath =
            "Assets/Scripts/Editor/UnityMcp/_CompileSentinel.cs";

        // File-based persistence (survives domain reload)
        private static string StateFilePath =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath), "Temp", "UnityMcpCompileState.json");
        
        public static CmdResult Execute(string method, Dictionary<string, string> args)
        {
            
            return method switch
            {
                "ping" => Ping(),
                "list_hierarchy" => ListHierarchy(args),
                "compile" => Compile(),
                "compile_status" => CompileStatus(),
                "cleanup_compile_state" => CleanupCompileState(),
                _ => CmdResult.Err($"Unknown method: {method}"),
            };
        }

        private static CmdResult Ping()
        {
            Debug.Log("gzp 接收ping指令");
            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "ok",
                ["unityVersion"] = Application.unityVersion,
                ["projectName"] = Application.productName,
            });
        }

        private static CmdResult Compile()
        {
            Debug.Log("gzp 接收编译指令");
            if (EditorApplication.isCompiling)
            {
                return CmdResult.Ok(new Dictionary<string, object>
                {
                    ["status"] = "already_compiling",
                    ["message"] = "A compilation is already in progress. Retry compile once it finishes.",
                });
            }

            // Touch sentinel file to ensure Unity always detects a change
            TouchSentinel();

            // Clean up any stale state file from a previous run
            DeleteCompileState();

            _compileErrors.Clear();
            _compileWarnings.Clear();
            _compileRequested = true;
            _hasStartedCompiling = false;
            _compileFinished = false;
            _compileRequestTime = EditorApplication.timeSinceStartup;

            // Persist state to survive domain reload during compilation
            WriteCompileState("in_progress", EditorApplication.timeSinceStartup);

            // Note: AssetDatabase.Refresh is deferred to TriggerCompileRefresh()
            // to allow the TCP response to be sent before domain reload kills the connection.

            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "compilation_started",
                ["message"] = "Compilation preparation done. Refresh pending.",
            });
        }

        /// <summary>
        /// Actually triggers AssetDatabase.Refresh to start compilation.
        /// Must be called AFTER the TCP response has been sent, since
        /// domain reload will kill the connection.
        /// </summary>
        public static void TriggerCompileRefresh()
        {
            if (!_compileRequested || EditorApplication.isCompiling) return;
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static CmdResult CompileStatus()
        {
            Debug.Log("gzp 检测编译状态");
            bool isCompiling = EditorApplication.isCompiling;

            // If in-memory state was wiped (domain reload), recover from file
            if (!_compileRequested && !_compileFinished)
            {
                var recovered = TryRecoverCompileState();
                if (recovered != null)
                {
                    if (recovered.Value.state == "done")
                    {
                        _compileFinished = true; // cache in memory
                        return CmdResult.Ok(new Dictionary<string, object>
                        {
                            ["isCompiling"] = false,
                            ["compileRequested"] = true,
                            ["compileFinished"] = true,
                            ["errorCount"] = recovered.Value.errorCount,
                            ["warningCount"] = recovered.Value.warningCount,
                            ["errors"] = recovered.Value.errors,
                            ["warnings"] = recovered.Value.warnings,
                        });
                    }
                    else if (!isCompiling)
                    {
                        // Was "in_progress" but isCompiling is false now —
                        // compilation completed via domain reload (0 errors)
                        WriteCompileState("done", recovered.Value.startTime);
                        _compileFinished = true;
                        return CmdResult.Ok(new Dictionary<string, object>
                        {
                            ["isCompiling"] = false,
                            ["compileRequested"] = true,
                            ["compileFinished"] = true,
                            ["errorCount"] = recovered.Value.errorCount,
                            ["warningCount"] = recovered.Value.warningCount,
                            ["errors"] = recovered.Value.errors,
                            ["warnings"] = recovered.Value.warnings,
                        });
                    }
                }
            }

            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["isCompiling"] = isCompiling,
                ["compileRequested"] = _compileRequested,
                ["compileFinished"] = _compileFinished,
                ["errorCount"] = _compileErrors.Count,
                ["warningCount"] = _compileWarnings.Count,
                ["errors"] = _compileErrors.Select(e => new Dictionary<string, object>
                {
                    ["file"] = e.file,
                    ["line"] = e.line,
                    ["column"] = e.column,
                    ["message"] = e.message,
                }).ToList(),
                ["warnings"] = _compileWarnings.Select(w => new Dictionary<string, object>
                {
                    ["file"] = w.file,
                    ["line"] = w.line,
                    ["column"] = w.column,
                    ["message"] = w.message,
                }).ToList(),
            });
        }

        /// <summary>Called by UnityMcpBridge when assembly compilation finishes.</summary>
        public static void CollectCompileMessages(string assemblyPath, CompilerMessage[] messages)
        {
            if (!_compileRequested || _compileFinished) return;

            foreach (var msg in messages)
            {
                if (msg.type == CompilerMessageType.Error)
                    _compileErrors.Add(msg);
                else if (msg.type == CompilerMessageType.Warning)
                    _compileWarnings.Add(msg);
            }

            // Persist to file so messages survive a domain reload
            AppendMessagesToFile(messages);

            // compilationFinished fires per-assembly — mark done when
            // EditorApplication.isCompiling transitions to false
        }

        /// <summary>Called by UnityMcpBridge each editor update to detect compile completion.</summary>
        public static void CheckCompileCompletion()
        {
            if (!_compileRequested || _compileFinished) return;

            if (EditorApplication.isCompiling)
            {
                _hasStartedCompiling = true;
                return;
            }

            // isCompiling is false now
            if (_hasStartedCompiling)
            {
                // Was compiling, now stopped → done
                _compileFinished = true;
                _compileRequested = false;
                WriteCompileState("done", _compileRequestTime, _compileErrors, _compileWarnings);
            }
            else if (EditorApplication.timeSinceStartup - _compileRequestTime > 5.0)
            {
                // No compilation started within 5s → no changes to compile
                _compileFinished = true;
                _compileRequested = false;
                DeleteCompileState();
            }
        }

        private static CmdResult ListHierarchy(Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return CmdResult.Err($"Prefab not found at: {prefabPath}");

            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["prefabPath"] = prefabPath,
                ["hierarchy"] = BuildTree(prefab.transform),
                ["totalObjects"] = CountAll(prefab.transform),
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

        // ── Compile state file persistence (survives domain reload) ─

        private static void WriteCompileState(string state, double startTime,
            List<CompilerMessage> errors = null, List<CompilerMessage> warnings = null)
        {
            try
            {
                var data = new CompileStateData
                {
                    state = state,
                    startTime = startTime,
                    errors = errors?.Select(FromCompilerMessage).ToList() ?? new List<CompileMsgEntry>(),
                    warnings = warnings?.Select(FromCompilerMessage).ToList() ?? new List<CompileMsgEntry>(),
                };
                var dir = Path.GetDirectoryName(StateFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(StateFilePath, JsonUtility.ToJson(data));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UnityMcp] Failed to write compile state file: {ex.Message}");
            }
        }

        private static void AppendMessagesToFile(CompilerMessage[] messages)
        {
            try
            {
                if (!File.Exists(StateFilePath)) return;
                var json = File.ReadAllText(StateFilePath);
                var data = JsonUtility.FromJson<CompileStateData>(json);
                if (data == null) return;

                foreach (var msg in messages)
                {
                    if (msg.type == CompilerMessageType.Error)
                        data.errors.Add(FromCompilerMessage(msg));
                    else if (msg.type == CompilerMessageType.Warning)
                        data.warnings.Add(FromCompilerMessage(msg));
                }
                File.WriteAllText(StateFilePath, JsonUtility.ToJson(data));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UnityMcp] Failed to append compile messages: {ex.Message}");
            }
        }

        private static (string state, double startTime, int errorCount, int warningCount,
            List<Dictionary<string, object>> errors, List<Dictionary<string, object>> warnings)?
            TryRecoverCompileState()
        {
            try
            {
                if (!File.Exists(StateFilePath)) return null;
                var json = File.ReadAllText(StateFilePath);
                var data = JsonUtility.FromJson<CompileStateData>(json);
                if (data == null) return null;

                var errors = data.errors?.Select(e => new Dictionary<string, object>
                {
                    ["file"] = e.file ?? "",
                    ["line"] = e.line,
                    ["column"] = e.column,
                    ["message"] = e.message ?? "",
                }).ToList() ?? new List<Dictionary<string, object>>();

                var warnings = data.warnings?.Select(w => new Dictionary<string, object>
                {
                    ["file"] = w.file ?? "",
                    ["line"] = w.line,
                    ["column"] = w.column,
                    ["message"] = w.message ?? "",
                }).ToList() ?? new List<Dictionary<string, object>>();

                return (data.state, data.startTime,
                    data.errors?.Count ?? 0, data.warnings?.Count ?? 0,
                    errors, warnings);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UnityMcp] Failed to recover compile state: {ex.Message}");
                return null;
            }
        }

        private static void DeleteCompileState()
        {
            try
            {
                if (File.Exists(StateFilePath))
                    File.Delete(StateFilePath);
            }
            catch { /* ignore */ }
        }

        /// <summary>
        /// Called by client after reading compile results from the state file.
        /// Cleans up both the state file and the in-memory compile tracking.
        /// </summary>
        private static CmdResult CleanupCompileState()
        {
            DeleteCompileState();
            _compileRequested = false;
            _compileFinished = false;
            _hasStartedCompiling = false;
            _compileErrors.Clear();
            _compileWarnings.Clear();
            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "cleaned",
            });
        }

        /// <summary>
        /// Writes a new timestamp to the sentinel file to force Unity to detect
        /// a script change and trigger actual compilation on AssetDatabase.Refresh.
        /// </summary>
        private static void TouchSentinel()
        {
            try
            {
                var path = Path.Combine(Path.GetDirectoryName(Application.dataPath), SentinelPath);
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path,
                    $"// Auto-generated compile sentinel — touched to force Unity recompilation.\n" +
                    $"// DO NOT EDIT MANUALLY.\n" +
                    $"// Last trigger: {DateTime.Now:O}\n");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UnityMcp] Failed to touch sentinel file: {ex.Message}");
            }
        }

        /// <summary>Public path to compile state file, for clients to read.</summary>
        public static string CompileStateFilePath =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath), "Temp", "UnityMcpCompileState.json");

        private static CompileMsgEntry FromCompilerMessage(CompilerMessage m) =>
            new() { file = m.file, line = m.line, column = m.column, message = m.message };

        [Serializable]
        private class CompileStateData
        {
            public string state;
            public double startTime;
            public List<CompileMsgEntry> errors;
            public List<CompileMsgEntry> warnings;
        }

        [Serializable]
        private class CompileMsgEntry
        {
            public string file;
            public int line;
            public int column;
            public string message;
        }

        private static int CountAll(Transform t)
        {
            int n = 1;
            for (int i = 0; i < t.childCount; i++)
                n += CountAll(t.GetChild(i));
            return n;
        }
    }

    public class CmdResult
    {
        public bool Success;
        public object Data;
        public string ErrMsg;

        public static CmdResult Ok(object data) => new() { Success = true, Data = data };
        public static CmdResult Err(string msg) => new() { Success = false, ErrMsg = msg };
    }
}
