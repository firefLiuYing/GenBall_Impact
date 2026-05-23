using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Yueyn.Editor.UnityMcp
{
    /// <summary>
    /// WebSocket client that connects to the Python MCP server.
    /// Receives commands on a background thread, executes them on the main thread,
    /// and sends responses back.
    ///
    /// Auto-starts on Unity Editor load via [InitializeOnLoad].
    /// </summary>
    [InitializeOnLoad]
    public static class UnityMcpBridge
    {
        private const string DefaultUrl = "ws://localhost:9876";
        private const int MaxMessageSize = 1 << 20; // 1 MB
        private const float ReconnectDelayMin = 1f;
        private const float ReconnectDelayMax = 30f;

        private static ClientWebSocket _ws;
        private static CancellationTokenSource _cts;
        private static bool _isRunning;
        private static float _reconnectTimer;
        private static float _currentReconnectDelay = ReconnectDelayMin;

        // Thread-safe queues
        private static readonly ConcurrentQueue<string> IncomingMessages = new();
        private static readonly ConcurrentQueue<string> OutgoingMessages = new();
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<string>> PendingRequests = new();

        static UnityMcpBridge()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            _ = ConnectAsync();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Disconnect();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                _ = ConnectAsync();
            }
        }

        private static async Task ConnectAsync()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cts = new CancellationTokenSource();

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    _ws?.Dispose();
                    _ws = new ClientWebSocket();
                    _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(10);

                    Debug.Log($"[UnityMcp] Connecting to {DefaultUrl}...");
                    await _ws.ConnectAsync(new Uri(DefaultUrl), _cts.Token);

                    Debug.Log($"[UnityMcp] Connected to MCP server");
                    _currentReconnectDelay = ReconnectDelayMin;

                    // Start receive loop
                    await ReceiveLoop(_cts.Token);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[UnityMcp] Connection failed: {ex.Message}. " +
                        $"Reconnecting in {_currentReconnectDelay:F0}s...");
                }

                if (_cts.IsCancellationRequested) break;

                // Exponential backoff
                try { await Task.Delay((int)(_currentReconnectDelay * 1000), _cts.Token); }
                catch (OperationCanceledException) { break; }

                _currentReconnectDelay = Mathf.Min(_currentReconnectDelay * 2, ReconnectDelayMax);
            }

            _isRunning = false;
        }

        private static async Task ReceiveLoop(CancellationToken ct)
        {
            var buffer = new byte[MaxMessageSize];

            while (_ws?.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                // Send queued outgoing messages
                while (OutgoingMessages.TryDequeue(out var msg))
                {
                    var bytes = Encoding.UTF8.GetBytes(msg);
                    await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text,
                        true, ct);
                }

                // Receive
                WebSocketReceiveResult result;
                var offset = 0;
                do
                {
                    result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer, offset,
                        buffer.Length - offset), ct);
                    offset += result.Count;
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("[UnityMcp] Server closed connection");
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, offset);
                IncomingMessages.Enqueue(json);
            }
        }

        /// <summary>
        /// Called by EditorApplication.update on the main thread.
        /// Processes incoming commands.
        /// </summary>
        private static void OnEditorUpdate()
        {
            // Process incoming commands
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

                OutgoingMessages.Enqueue(sb.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityMcp] Command processing error: {ex}");
            }
        }

        // ── Minimal JSON helpers (no library dependency) ──────────────

        private static string ExtractString(string json, string key)
        {
            // Match "key":"value" — value can be any string up to unescaped "
            var search = $"\"{key}\":\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return "";
            idx += search.Length;
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
            var search = $"\"{key}\":{{";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return result;

            idx += search.Length;
            // Simple key-value extraction for flat string params
            while (idx < json.Length && json[idx] != '}')
            {
                // Skip whitespace and commas
                while (idx < json.Length && (json[idx] == ' ' || json[idx] == ',' || json[idx] == '\n'))
                    idx++;
                if (idx >= json.Length || json[idx] == '}') break;

                // Extract key
                if (json[idx] != '"') break;
                idx++; // skip opening "
                var keyStart = idx;
                while (idx < json.Length && json[idx] != '"') idx++;
                var paramKey = json.Substring(keyStart, idx - keyStart);
                idx++; // skip closing "
                idx++; // skip :

                // Extract value (string only for now)
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
                    idx++; // skip closing "
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

        public static void Disconnect()
        {
            _cts?.Cancel();
            try { _ws?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).Wait(1000); }
            catch { /* ignore */ }
            _ws?.Dispose();
            _ws = null;
            _isRunning = false;
        }
    }
}
