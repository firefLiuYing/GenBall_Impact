#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GenBall.Event.Editor
{
    [CustomEditor(typeof(PlacedEventTable))]
    public class PlacedEventTableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var table = (PlacedEventTable)target;

            EditorGUILayout.LabelField($"Entries: {table.entries.Count}", EditorStyles.boldLabel);

            // Validation status
            var conflictError = table.ValidateNoConflicts();
            if (conflictError != null)
            {
                EditorGUILayout.HelpBox(conflictError, MessageType.Error);
            }
            else if (table.entries.Count > 0)
            {
                EditorGUILayout.HelpBox("All placed events valid — no ID conflicts.", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Reimport button
            if (GUILayout.Button("Reimport from CSV", GUILayout.Height(30)))
            {
                PlacedEventCsvImporter.ImportFromCsv();
            }

            EditorGUILayout.Space();

            // Read-only entry list
            if (table.entries.Count > 0)
            {
                EditorGUILayout.LabelField("Entries (read-only — edit the CSV):", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(true);
                for (int i = 0; i < table.entries.Count; i++)
                {
                    var e = table.entries[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"[{e.id}]", GUILayout.Width(70));
                    EditorGUILayout.LabelField(e.displayName, GUILayout.Width(120));
                    EditorGUILayout.LabelField(e.name, GUILayout.Width(140));
                    var paramLabel = string.IsNullOrEmpty(e.defaultParamType) ? "(none)" : e.defaultParamType;
                    EditorGUILayout.LabelField(paramLabel);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("No entries. Click 'Reimport from CSV' to load placed events.", MessageType.Warning);
            }
        }
    }
}
#endif
