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
    /// Receives commands on background threads, executes them on the main
    /// thread, and sends responses back.
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
        private static readonly ConcurrentQueue<string> IncomingMessages =
            new();
        private static readonly BlockingCollection<string> OutgoingMessages =
            new();

        private static bool _compileEventsSubscribed;

        static UnityMcpBridge()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            // Don't start the server if we're in a domain reload that's
            // part of entering Play Mode. The EnteredEditMode handler will
            // start it when we return to Edit Mode.
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                StartServer();
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    Disconnect();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    Disconnect();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    StartServer();
                    break;
            }
        }

        // ── Server lifecycle ──────────────────────────────────────────

        private static void StartServer()
        {
            if (_isRunning) return;

            // Clean up any stale listener reference from this domain
            if (_listener != null)
            {
                try { _listener.Stop(); }
                catch { /* ignore */ }
                _listener = null;
            }

            // After domain reload, the old AppDomain's background thread
            // may still hold port 9876 in a blocking Accept. Connect to
            // it briefly to unblock the old thread, causing it to crash
            // (its user-code methods are gone) and release the port.
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using (var knock = new TcpClient("127.0.0.1", Port))
                    {
                        // Connected — old thread's AcceptTcpClient
                        // returned, and it will crash when trying to
                        // call ReceiveLoop (unloaded assembly).
                    }
                }
                catch
                {
                    // Port is not connectable → old listener is gone
                    break;
                }
                Thread.Sleep(500);
            }

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
            TcpListener listener = null;

            // Retry binding — stale socket may take a moment to release
            for (int retry = 0; retry < 5; retry++)
            {
                try
                {
                    listener = new TcpListener(IPAddress.Loopback, Port);
                    listener.Server.SetSocketOption(
                        SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress, true);
                    listener.Start();
                    _listener = listener;
                    _isRunning = true;
#if UNITY_MCP_VERBOSE
                    Debug.Log($"[UnityMcp] Listening on port {Port}");
#endif
                    break;
                }
                catch (SocketException ex)
                {
                    Debug.LogWarning(
                        $"[UnityMcp] Bind attempt {retry + 1}/5: "
                        + ex.Message);
                    try { listener?.Server?.Close(); } catch { }
                    listener = null;
                    if (retry < 4)
                    {
                        try
                        {
                            using (var _ = new TcpClient(
                                "127.0.0.1", Port)) { }
                        }
                        catch { }
                        Thread.Sleep(1000);
                    }
                }
            }

            if (listener == null)
            {
                Debug.LogError(
                    $"[UnityMcp] Failed to listen on port {Port} "
                    + "after 5 attempts.");
                _isRunning = false;
                return;
            }

            try
            {
                while (_isRunning && !_cts.IsCancellationRequested)
                {
                    TcpClient newClient = null;
                    try
                    {
                        if (_listener.Pending())
                            newClient = _listener.AcceptTcpClient();
                    }
                    catch (SocketException) { break; }
                    catch (ObjectDisposedException) { break; }
                    catch (InvalidOperationException) { break; }

                    if (newClient != null)
                    {
                        // Accept only one connection at a time
                        lock (ClientLock)
                        {
                            if (_client != null)
                            {
                                Debug.LogWarning(
                                    "[UnityMcp] Rejecting new connection: "
                                    + "already connected");
                                newClient.Close();
                                continue;
                            }
                            _client = newClient;
                        }

                        newClient.ReceiveTimeout = 60000;
                        newClient.SendTimeout = 30000;

#if UNITY_MCP_VERBOSE
                        Debug.Log("[UnityMcp] Client connected");
#endif

                        using (var connCts =
                            new CancellationTokenSource())
                        {
                            var receiveThread = new Thread(() =>
                                ReceiveLoop(newClient, connCts.Token))
                            {
                                IsBackground = true,
                                Name = "UnityMcp-Receive"
                            };
                            var sendThread = new Thread(() =>
                                SendLoop(newClient, connCts.Token))
                            {
                                IsBackground = true,
                                Name = "UnityMcp-Send"
                            };

                            receiveThread.Start();
                            sendThread.Start();

                            // Wait for receive to finish
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

#if UNITY_MCP_VERBOSE
                        Debug.Log("[UnityMcp] Client disconnected");
#endif
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            finally
            {
                if (ReferenceEquals(_listener, listener))
                {
                    _isRunning = false;
                    _listener = null;
                }
                try { listener.Stop(); }
                catch { /* ignore */ }
            }
        }

        // ── Receive / Send loops ──────────────────────────────────────

        private static void ReceiveLoop(
            TcpClient client, CancellationToken ct)
        {
            try
            {
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(
                    stream, Encoding.UTF8, false, MaxMessageSize))
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

                        if (line == null) break;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        IncomingMessages.Enqueue(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[UnityMcp] Receive error: {ex.Message}");
            }
        }

        private static void SendLoop(
            TcpClient client, CancellationToken ct)
        {
            try
            {
                using (var stream = client.GetStream())
                using (var writer = new StreamWriter(
                    stream, Encoding.UTF8, MaxMessageSize)
                    { AutoFlush = false })
                {
                    while (!ct.IsCancellationRequested)
                    {
                        string line;
                        try
                        {
                            if (!OutgoingMessages.TryTake(
                                out line, 500, ct))
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
                Debug.LogWarning(
                    $"[UnityMcp] Send error: {ex.Message}");
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
                // ── Parse request envelope via JsonUtility ──
                var envelope = JsonUtility.FromJson<JsonRpcEnvelope>(json);
                var requestId = envelope.id ?? "";
                var method = envelope.method ?? "";

                // ── Parse params into Dictionary<string, string> ──
                var args = ExtractParamsDict(json);

#if UNITY_MCP_VERBOSE
                Debug.Log($"[UnityMcp] ← {method}");
#endif

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

                if (method == "compile")
                {
                    // Compile triggers domain reload via
                    // AssetDatabase.Refresh. Use synchronous send on the
                    // main thread so the response is guaranteed to reach
                    // the client before the reload kills the background
                    // SendLoop thread.
                    SendResponseSync(responseJson);
                    UnityCommandHandler.TriggerCompileRefresh();
                }
                else
                {
                    OutgoingMessages.Add(responseJson);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[UnityMcp] Command processing error: {ex}");
            }
        }

        /// <summary>
        /// Synchronously writes a response line to the TCP stream on the
        /// current thread. Used for commands (like compile) that trigger
        /// domain reload, where the background SendLoop thread may not
        /// flush in time.
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
                    Debug.LogWarning(
                        "[UnityMcp] Failed to send sync response: "
                        + ex.Message);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  JSON helpers
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Extract the "params" JSON substring and parse it into a flat
        /// <c>Dictionary&lt;string, string&gt;</c>.
        ///
        /// Uses balanced-brace counting so it correctly handles nested
        /// objects/arrays inside the params block (unlike the old
        /// character-by-character scanner).
        /// </summary>
        private static Dictionary<string, string> ExtractParamsDict(
            string json)
        {
            var result = new Dictionary<string, string>();
            var paramsJson = ExtractJsonSubstring(json, "params");
            if (string.IsNullOrEmpty(paramsJson))
                return result;

            return ParseFlatStringDict(paramsJson);
        }

        /// <summary>
        /// Extract a named JSON object substring using balanced-brace
        /// counting.  Correctly tracks string state so braces/colons
        /// inside string literals are ignored.
        ///
        /// Example: given <c>{"params":{"key":"val"}}</c> and key
        /// <c>"params"</c>, returns <c>{"key":"val"}</c>.
        /// </summary>
        public static string ExtractJsonSubstring(
            string json, string key)
        {
            var search = $"\"{key}\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;

            idx += search.Length;
            // Skip whitespace and colon
            while (idx < json.Length)
            {
                var c = json[idx];
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                { idx++; continue; }
                if (c == ':') { idx++; break; }
                // Unexpected character
                return null;
            }
            // Skip whitespace after colon
            while (idx < json.Length)
            {
                var c = json[idx];
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                { idx++; continue; }
                break;
            }
            if (idx >= json.Length || json[idx] != '{')
                return null;

            return ExtractBalancedBraces(json, idx);
        }

        /// <summary>
        /// Extract a balanced-brace substring starting at <c>start</c>
        /// (which must point to '{').  Tracks string/escape state so
        /// braces inside string literals are ignored.
        /// </summary>
        private static string ExtractBalancedBraces(
            string json, int start)
        {
            if (json[start] != '{') return null;

            int depth = 1;
            int i = start + 1;
            bool inString = false;

            while (i < json.Length && depth > 0)
            {
                char c = json[i];
                if (inString)
                {
                    if (c == '\\') i++; // skip escaped char
                    else if (c == '"') inString = false;
                }
                else
                {
                    if (c == '"') inString = true;
                    else if (c == '{') depth++;
                    else if (c == '}') depth--;
                }
                i++;
            }

            return json.Substring(start, i - start);
        }

        /// <summary>
        /// Parse a flat JSON object whose values are all strings into a
        /// <c>Dictionary&lt;string, string&gt;</c>.  Handles escape
        /// sequences (<c>\\</c>, <c>\"</c>, <c>\n</c>, <c>\r</c>,
        /// <c>\t</c>, <c>\uXXXX</c>).
        /// </summary>
        private static Dictionary<string, string> ParseFlatStringDict(
            string json)
        {
            var result = new Dictionary<string, string>();
            if (json.Length < 2 || json[0] != '{')
                return result;

            int i = 1;
            while (i < json.Length)
            {
                // Skip whitespace, commas, newlines
                while (i < json.Length)
                {
                    var c = json[i];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r'
                        || c == ',')
                    { i++; continue; }
                    break;
                }
                if (i >= json.Length || json[i] == '}')
                    break;

                // Read key (must be a string)
                if (json[i] != '"') break;
                i++;
                var key = ReadJsonString(json, ref i);
                if (key == null) break;

                // Skip colon
                while (i < json.Length)
                {
                    var c = json[i];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                    { i++; continue; }
                    if (c == ':') { i++; break; }
                    break;
                }

                // Skip whitespace after colon
                while (i < json.Length)
                {
                    var c = json[i];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                    { i++; continue; }
                    break;
                }

                // Read value — currently only string values are supported
                // for the Dictionary<string, string> interface.
                if (i < json.Length && json[i] == '"')
                {
                    i++;
                    var value = ReadJsonString(json, ref i);
                    if (value != null)
                        result[key] = value;
                }
                else
                {
                    // Skip non-string values (numbers, booleans, null,
                    // nested objects, arrays) — they can't go into
                    // Dictionary<string, string>.
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Read a JSON string starting just after the opening quote.
        /// Advances <c>i</c> past the closing quote.  Returns null on
        /// parse failure.  Handles escape sequences.
        /// </summary>
        private static string ReadJsonString(string json, ref int i)
        {
            var sb = new StringBuilder();
            while (i < json.Length)
            {
                var c = json[i];
                if (c == '\\')
                {
                    i++;
                    if (i >= json.Length) return null;
                    var esc = json[i];
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            // \uXXXX
                            if (i + 4 >= json.Length) return null;
                            var hex = json.Substring(i + 1, 4);
                            if (int.TryParse(hex,
                                System.Globalization.NumberStyles.HexNumber,
                                null, out var codePoint))
                            {
                                sb.Append((char)codePoint);
                                i += 4;
                            }
                            else return null;
                            break;
                        default: return null;
                    }
                }
                else if (c == '"')
                {
                    i++; // skip closing quote
                    return sb.ToString();
                }
                else
                {
                    sb.Append(c);
                }
                i++;
            }
            return null; // unterminated string
        }

        // ── JSON serialization (response building) ────────────────────

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
            if (obj is float f)
                return f.ToString(
                    System.Globalization.CultureInfo.InvariantCulture);
            return $"\"{EscapeJson(obj?.ToString() ?? "")}\"";
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }

        // ── Public helpers ─────────────────────────────────────────────

        public static void EnqueueCommand(
            string method, Dictionary<string, string> args)
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
                    sb.Append(
                        $"\"{EscapeJson(kvp.Key)}\":"
                        + $"\"{EscapeJson(kvp.Value)}\"");
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
