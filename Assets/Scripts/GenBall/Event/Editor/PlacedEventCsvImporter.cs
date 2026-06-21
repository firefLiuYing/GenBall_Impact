#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GenBall.Event.Editor
{
    /// <summary>
    /// Imports placed events from a CSV file into PlacedEventTable.asset.
    /// CSV path: Assets/Editor/Configs/PlacedEvents.csv
    /// </summary>
    public static class PlacedEventCsvImporter
    {
        private const string CsvPath = "Assets/Editor/Configs/PlacedEvents.csv";
        private const string AssetPath = "Assets/Resources/Configs/PlacedEventTable.asset";
        private const string ConfigDir = "Assets/Resources/Configs";

        [MenuItem("Tools/Import Placed Events")]
        public static void ImportFromCsv()
        {
            if (!File.Exists(CsvPath))
            {
                Debug.LogError($"[PlacedEventCsvImporter] CSV not found at: {CsvPath}");
                return;
            }

            var entries = ParseCsv(CsvPath);
            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning("[PlacedEventCsvImporter] CSV is empty or could not be parsed.");
                return;
            }

            var table = GetOrCreateTable();
            table.entries = entries;

            // Validate
            var conflictError = table.ValidateNoConflicts();
            if (conflictError != null)
            {
                Debug.LogError($"[PlacedEventCsvImporter] Validation failed: {conflictError}");
                EditorUtility.DisplayDialog("Placed Event Import Failed",
                    $"Validation error:\n\n{conflictError}", "OK");
                return;
            }

            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PlacedEventCsvImporter] Imported {entries.Count} placed events from CSV.");
            EditorUtility.DisplayDialog("Placed Event Import",
                $"Successfully imported {entries.Count} placed events.", "OK");
        }

        private static List<PlacedEventEntry> ParseCsv(string path)
        {
            var entries = new List<PlacedEventEntry>();
            var lines = File.ReadAllLines(path);

            if (lines.Length < 2)
            {
                Debug.LogWarning("[PlacedEventCsvImporter] CSV has no data rows.");
                return entries;
            }

            // Skip header line (index 0)
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var fields = ParseCsvLine(line);
                if (fields.Length < 3)
                {
                    Debug.LogWarning($"[PlacedEventCsvImporter] Skipping line {i + 1}: insufficient fields.");
                    continue;
                }

                if (!int.TryParse(fields[0].Trim(), out var id))
                {
                    Debug.LogWarning($"[PlacedEventCsvImporter] Skipping line {i + 1}: invalid ID '{fields[0]}'.");
                    continue;
                }

                var entry = new PlacedEventEntry
                {
                    id = id,
                    name = fields[1].Trim(),
                    displayName = fields[2].Trim(),
                    defaultParamType = fields.Length > 3 ? fields[3].Trim() : string.Empty,
                };
                entries.Add(entry);
            }

            return entries;
        }

        private static string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            int start = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ',' && !inQuotes)
                {
                    fields.Add(line.Substring(start, i - start).Trim('"'));
                    start = i + 1;
                }
            }
            fields.Add(line.Substring(start).Trim('"'));
            return fields.ToArray();
        }

        private static PlacedEventTable GetOrCreateTable()
        {
            var table = AssetDatabase.LoadAssetAtPath<PlacedEventTable>(AssetPath);
            if (table != null) return table;

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(ConfigDir))
                AssetDatabase.CreateFolder("Assets/Resources", "Configs");

            table = ScriptableObject.CreateInstance<PlacedEventTable>();
            AssetDatabase.CreateAsset(table, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[PlacedEventCsvImporter] Created new PlacedEventTable at {AssetPath}");

            return table;
        }
    }
}
#endif
