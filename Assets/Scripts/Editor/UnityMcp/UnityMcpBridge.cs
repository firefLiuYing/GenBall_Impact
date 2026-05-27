using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Yueyn.Editor.UnityMcp
{
    /// <summary>
    /// TCP server that listens for connections from the Python MCP bridge.
    /// Receives commands on background threads, executes them on the main thread,
    /// and sends responses back.
    ///
    /// Auto-starts on Unity Editor load via [InitializeOnLoad].
    /// </summary>
    [InitializeOnLoad]
    public static class UnityMcpBridge
    {
        private const int Port = 9876;
        private const int MaxMessageSize = 1 << 20; // 1 MB

        private static TcpListener _listener;
        private static TcpClient _client;
        private static readonly object ClientLock = new();
        private static volatile bool _isRunning;
        private static CancellationTokenSource _cts;

        // Thread-safe queues
        private static readonly ConcurrentQueue<string> IncomingMessages = new();
        private static readonly BlockingCollection<string> OutgoingMessages = new();

        private static bool _compileEventsSubscribed;

        static UnityMcpBridge()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            StartServer();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Disconnect();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                StartServer();
            }
        }

        // ── Server lifecycle ──────────────────────────────────────────

        private static void StartServer()
        {
            if (_isRunning) return;

            _cts = new CancellationTokenSource();

            var acceptThread = new Thread(AcceptLoop)
            {
                IsBackground = true,
                Name = "UnityMcp-Accept"
            };
            acceptThread.Start();
        }

        private static void AcceptLoop()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, Port);
                _listener.Start();
                _isRunning = true;
                Debug.Log($"[UnityMcp] Listening on port {Port}");
            }
            catch (SocketException ex)
            {
                Debug.LogError($"[UnityMcp] Failed to listen on port {Port}: {ex.Message}. " +
                    "Check if another process is using this port.");
                _isRunning = false;
                return;
            }

            while (_isRunning && !_cts.IsCancellationRequested)
            {
                TcpClient newClient;
                try
                {
                    newClient = _listener.AcceptTcpClient();
                }
                catch (SocketException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (InvalidOperationException) { break; }

                // Accept only one connection at a time
                lock (ClientLock)
                {
                    if (_client != null)
                    {
                        Debug.LogWarning("[UnityMcp] Rejecting new connection: already connected");
                        newClient.Close();
                        continue;
                    }
                    _client = newClient;
                }

                // Set timeouts to detect stale connections
                newClient.ReceiveTimeout = 60000;
                newClient.SendTimeout = 30000;

                Debug.Log("[UnityMcp] Client connected");

                // Per-connection cancellation
                using (var connCts = new CancellationTokenSource())
                {
                    var receiveThread = new Thread(() => ReceiveLoop(newClient, connCts.Token))
                    {
                        IsBackground = true,
                        Name = "UnityMcp-Receive"
                    };
                    var sendThread = new Thread(() => SendLoop(newClient, connCts.Token))
                    {
                        IsBackground = true,
                        Name = "UnityMcp-Send"
                    };

                    receiveThread.Start();
                    sendThread.Start();

                    // Wait for receive to finish (means client disconnected)
                    receiveThread.Join();

                    // Signal send thread to stop
                    connCts.Cancel();
                    sendThread.Join(3000);
                }

                lock (ClientLock)
                {
                    try { _client?.Close(); }
                    catch { /* ignore */ }
                    try { _client?.Dispose(); }
                    catch { /* ignore */ }
                    _client = null;
                }

                Debug.Log("[UnityMcp] Client disconnected");
            }

            _isRunning = false;
        }

        // ── Receive / Send loops ──────────────────────────────────────

        private static void ReceiveLoop(TcpClient client, CancellationToken ct)
        {
            try
            {
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8, false, MaxMessageSize))
                {
                    while (!ct.IsCancellationRequested)
                    {
                        string line;
                        try
                        {
                            line = reader.ReadLine();
                        }
                        catch (IOException) { break; }
                        catch (ObjectDisposedException) { break; }

                        if (line == null) break; // stream closed
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        IncomingMessages.Enqueue(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[UnityMcp] Receive error: {ex.Message}");
            }
        }

        private static void SendLoop(TcpClient client, CancellationToken ct)
        {
            try
            {
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(stream, Encoding.UTF8, MaxMessageSize) { AutoFlush = false })
                {
                    while (!ct.IsCancellationRequested)
                    {
                        string line;
                        try
                        {
                            if (!OutgoingMessages.TryTake(out line, 500, ct))
                                continue;
                        }
                        catch (OperationCanceledException) { break; }

                        try
                        {
                            writer.Write(line);
                            writer.Write('\n');
                            writer.Flush();
                        }
                        catch (IOException) { break; }
                        catch (ObjectDisposedException) { break; }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[UnityMcp] Send error: {ex.Message}");
            }
        }

        // ── Main thread processing ────────────────────────────────────

        private static void OnEditorUpdate()
        {
            // Subscribe to compilation events once
            if (!_compileEventsSubscribed)
            {
                _compileEventsSubscribed = true;
                CompilationPipeline.assemblyCompilationFinished +=
                    UnityCommandHandler.CollectCompileMessages;
            }

            UnityCommandHandler.CheckCompileCompletion();

            while (IncomingMessages.TryDequeue(out var json))
            {
                ProcessCommand(json);
            }
        }

        private static void ProcessCommand(string json)
        {
            try
            {
                var requestId = ExtractString(json, "id");
                var method = ExtractString(json, "method");
                var args = ExtractParams(json, "params");

                Debug.Log($"[UnityMcp] ← {method}");

                var result = UnityCommandHandler.Execute(method, args);

                var sb = new StringBuilder();
                sb.Append("{\"id\":\"");
                sb.Append(requestId);
                sb.Append("\"");
                if (result.Success)
                {
                    sb.Append(",\"result\":");
                    sb.Append(ToJson(result.Data));
                }
                else
                {
                    sb.Append(",\"error\":{\"message\":\"");
                    sb.Append(EscapeJson(result.ErrMsg ?? "unknown error"));
                    sb.Append("\"}");
                }
                sb.Append("}");

                var responseJson = sb.ToString();

                // For "compile", send the response synchronously on the main thread
                // BEFORE triggering AssetDatabase.Refresh. Domain reload during
                // compilation kills the TCP connection, so the background SendLoop
                // thread may not have time to flush. Writing directly here guarantees
                // the Python bridge receives the response and enters its polling loop.
                if (method == "compile")
                {
                    SendResponseSync(responseJson);
                    // Now safe to trigger compilation — domain reload can happen
                    UnityCommandHandler.TriggerCompileRefresh();
                }
                else
                {
                    OutgoingMessages.Add(responseJson);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMcp] Command processing error: {ex}");
            }
        }

        /// <summary>
        /// Synchronously writes a response line to the TCP stream on the current thread.
        /// Used for commands (like compile) that trigger domain reload, where the
        /// background SendLoop thread may not flush in time.
        /// </summary>
        private static void SendResponseSync(string response)
        {
            lock (ClientLock)
            {
                if (_client == null || !_client.Connected) return;
                try
                {
                    var stream = _client.GetStream();
                    var bytes = Encoding.UTF8.GetBytes(response + "\n");
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[UnityMcp] Failed to send sync response: {ex.Message}");
                }
            }
        }

        // ── Minimal JSON helpers (no library dependency) ──────────────

        private static string ExtractString(string json, string key)
        {
            // Search for "key" then skip optional whitespace, colon, and optional whitespace
            var keySearch = $"\"{key}\"";
            var idx = json.IndexOf(keySearch, StringComparison.Ordinal);
            if (idx < 0) return "";
            idx += keySearch.Length;
            // Skip whitespace, colon, whitespace
            while (idx < json.Length && (json[idx] == ' ' || json[idx] == '\t')) idx++;
            if (idx < json.Length && json[idx] == ':') idx++;
            while (idx < json.Length && (json[idx] == ' ' || json[idx] == '\t')) idx++;
            if (idx >= json.Length || json[idx] != '"') return "";
            idx++; // skip opening quote
            var end = idx;
            while (end < json.Length)
            {
                if (json[end] == '\\') { end += 2; continue; }
                if (json[end] == '"') break;
                end++;
            }
            return json.Substring(idx, end - idx);
        }

        private static Dictionary<string, string> ExtractParams(string json, string key)
        {
            var result = new Dictionary<string, string>();
            // Search for "key" then skip optional whitespace, colon, whitespace, opening brace
            var keySearch = $"\"{key}\"";
            var idx = json.IndexOf(keySearch, StringComparison.Ordinal);
            if (idx < 0) return result;

            idx += keySearch.Length;
            while (idx < json.Length && (json[idx] == ' ' || json[idx] == '\t')) idx++;
            if (idx < json.Length && json[idx] == ':') idx++;
            while (idx < json.Length && (json[idx] == ' ' || json[idx] == '\t')) idx++;
            if (idx >= json.Length || json[idx] != '{') return result;
            idx++; // skip {
            while (idx < json.Length && json[idx] != '}')
            {
                while (idx < json.Length && (json[idx] == ' ' || json[idx] == ',' || json[idx] == '\n'))
                    idx++;
                if (idx >= json.Length || json[idx] == '}') break;

                if (json[idx] != '"') break;
                idx++;
                var keyStart = idx;
                while (idx < json.Length && json[idx] != '"') idx++;
                var paramKey = json.Substring(keyStart, idx - keyStart);
                idx++;
                idx++;

                while (idx < json.Length && (json[idx] == ' ' || json[idx] == '\n')) idx++;
                if (json[idx] == '"')
                {
                    idx++;
                    var valStart = idx;
                    while (idx < json.Length)
                    {
                        if (json[idx] == '\\') { idx += 2; continue; }
                        if (json[idx] == '"') break;
                        idx++;
                    }
                    result[paramKey] = json.Substring(valStart, idx - valStart);
                    idx++;
                }
            }
            return result;
        }

        private static string ToJson(object obj)
        {
            if (obj is Dictionary<string, object> dict)
            {
                var sb = new StringBuilder("{");
                var first = true;
                foreach (var kvp in dict)
                {
                    if (!first) sb.Append(",");
                    sb.Append($"\"{EscapeJson(kvp.Key)}\":");
                    sb.Append(ToJson(kvp.Value));
                    first = false;
                }
                sb.Append("}");
                return sb.ToString();
            }
            if (obj is List<Dictionary<string, object>> list)
            {
                var sb = new StringBuilder("[");
                for (int n = 0; n < list.Count; n++)
                {
                    if (n > 0) sb.Append(",");
                    sb.Append(ToJson(list[n]));
                }
                sb.Append("]");
                return sb.ToString();
            }
            if (obj is List<string> strList)
            {
                var sb = new StringBuilder("[");
                for (int m = 0; m < strList.Count; m++)
                {
                    if (m > 0) sb.Append(",");
                    sb.Append($"\"{EscapeJson(strList[m])}\"");
                }
                sb.Append("]");
                return sb.ToString();
            }
            if (obj is string s) return $"\"{EscapeJson(s)}\"";
            if (obj is bool b) return b ? "true" : "false";
            if (obj is int ii) return ii.ToString();
            if (obj is long l) return l.ToString();
            if (obj is float f) return f.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return $"\"{EscapeJson(obj?.ToString() ?? "")}\"";
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"")
                    .Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        // ── Public helpers ─────────────────────────────────────────────

        public static void EnqueueCommand(string method, Dictionary<string, string> args)
        {
            var sb = new StringBuilder();
            sb.Append("{\"id\":\"editor_btn\",\"method\":\"");
            sb.Append(method);
            sb.Append("\"");
            if (args != null && args.Count > 0)
            {
                sb.Append(",\"params\":{");
                var first = true;
                foreach (var kvp in args)
                {
                    if (!first) sb.Append(",");
                    sb.Append($"\"{EscapeJson(kvp.Key)}\":\"{EscapeJson(kvp.Value)}\"");
                    first = false;
                }
                sb.Append("}");
            }
            sb.Append("}");
            IncomingMessages.Enqueue(sb.ToString());
        }

        // ── Shutdown ──────────────────────────────────────────────────

        public static void Disconnect()
        {
            _cts?.Cancel();

            lock (ClientLock)
            {
                try { _client?.Close(); }
                catch { /* ignore */ }
                try { _client?.Dispose(); }
                catch { /* ignore */ }
                _client = null;
            }

            try { _listener?.Stop(); }
            catch { /* ignore */ }
            _listener = null;
            _isRunning = false;

            // Flush outgoing queue so send thread can exit
            while (OutgoingMessages.TryTake(out _)) { }
        }
    }
}
