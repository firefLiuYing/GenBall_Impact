using UnityEditor;
using UnityEngine;

namespace Yueyn.Editor.UnityMcp
{
    public class McpDebugWindow : EditorWindow
    {
        [MenuItem("Tools/MCP Debug Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<McpDebugWindow>();
            window.titleContent = new GUIContent("MCP Debug");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("MCP Bridge Debug", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Compile", GUILayout.Height(40)))
            {
                UnityMcpBridge.EnqueueCommand("compile", null);
                Debug.Log("[McpDebug] Compile command enqueued");
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Ping", GUILayout.Height(40)))
            {
                UnityMcpBridge.EnqueueCommand("ping", null);
                Debug.Log("[McpDebug] Ping command enqueued");
            }
        }
    }
}
