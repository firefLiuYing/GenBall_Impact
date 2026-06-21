#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Event;
using GenBall.Event.Editor;
using UnityEditor;
using UnityEngine;

namespace GenBall.Map.Editor
{
    [CustomEditor(typeof(TriggerVolume))]
    public class TriggerVolumeEditor : UnityEditor.Editor
    {
        private List<SearchableEventPopup.EventEntry> _allEvents = new();

        public override void OnInspectorGUI()
        {
            var volume = (TriggerVolume)target;
            serializedObject.Update();

            // Trigger name
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("triggerName"),
                new GUIContent("Name"));
            EditorGUILayout.Space();

            // ── Event ──
            EditorGUILayout.LabelField("On Enter Event", EditorStyles.boldLabel);
            DrawEventIdDropdown(volume);
            DrawParameterSection(volume);
            EditorGUILayout.Space();

            // ── Collision settings ──
            EditorGUILayout.LabelField("Trigger Shape", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("radius"),
                new GUIContent("Radius"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("triggerBehavior"),
                new GUIContent("Behavior"));
            var behavior = (TriggerBehavior)serializedObject.FindProperty("triggerBehavior").intValue;
            if (behavior == TriggerBehavior.Limited)
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("maxFireCount"),
                    new GUIContent("Max Fires"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("cooldownSeconds"),
                new GUIContent("Cooldown (s)"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("targetLayers"),
                new GUIContent("Target Layers"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("spawnPoint"),
                new GUIContent("Spawn Point"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEventIdDropdown(TriggerVolume volume)
        {
            BuildEventList();

            var adapter = volume.OnEnter;
            var curId = adapter.EventId;
            var curEntry = _allEvents.FirstOrDefault(e => e.id == curId);
            var currentLabel = curId != 0 && curEntry.label != null
                ? curEntry.label
                : (curId != 0 ? $"[{curId}] Unknown" : "(none)");

            var rect = EditorGUILayout.GetControlRect();
            var labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
            var buttonRect = new Rect(rect.x + EditorGUIUtility.labelWidth, rect.y,
                rect.width - EditorGUIUtility.labelWidth, rect.height);

            EditorGUI.LabelField(labelRect, "Event ID");

            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = curId != 0 ? new Color(0.3f, 0.5f, 0.8f, 0.3f) : Color.clear;
            if (GUI.Button(buttonRect, currentLabel, EditorStyles.popup))
            {
                SearchableEventPopup.Show(buttonRect, _allEvents, curId,
                    selectedId =>
                    {
                        volume.OnEnter.EventId = selectedId;
                        TrySuggestParams(volume, selectedId);
                        EditorUtility.SetDirty(volume);
                    });
            }
            GUI.backgroundColor = oldBg;
        }

        private void DrawParameterSection(TriggerVolume volume)
        {
            var adapter = volume.OnEnter;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parameters", GUILayout.Width(120));
            if (adapter.HasParameters)
            {
                EditorGUILayout.LabelField(adapter.Parameters.GetType().Name, EditorStyles.miniLabel);
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                {
                    adapter.Parameters = null;
                    EditorUtility.SetDirty(volume);
                }
            }
            else
            {
                EditorGUILayout.LabelField("(none)", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            if (adapter.HasParameters)
            {
                EditorGUI.indentLevel++;
                var eventDataProp = serializedObject.FindProperty("onEnter");
                var paramsProp = eventDataProp?.FindPropertyRelative("parameters");
                if (paramsProp != null)
                    EditorGUILayout.PropertyField(paramsProp, new GUIContent(""), true);
                EditorGUI.indentLevel--;
            }
            else if (adapter.EventId != 0)
            {
                var suggestedTypes = EventParameterTypeDiscovery.GetSuggestedTypes(adapter.EventId);
                if (suggestedTypes.Count > 0 && GUILayout.Button("Add Parameter...", GUILayout.Height(22)))
                    ShowParamTypeMenu(volume, suggestedTypes);
            }
        }

        private void ShowParamTypeMenu(TriggerVolume volume, List<Type> types)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("(No Parameters)"), true, () =>
            {
                volume.OnEnter.Parameters = null;
                EditorUtility.SetDirty(volume);
            });
            menu.AddSeparator("");
            foreach (var t in types)
            {
                var captured = t;
                var hints = t.GetCustomAttributes(typeof(EventParamHintAttribute), false)
                    .Cast<EventParamHintAttribute>();
                var suffix = hints.Any()
                    ? $" ({string.Join(", ", hints.Select(h => h.EventId))})"
                    : "";
                menu.AddItem(new GUIContent($"{t.Name}{suffix}"), false, () =>
                {
                    volume.OnEnter.Parameters = (EventParameterBase)Activator.CreateInstance(captured);
                    EditorUtility.SetDirty(volume);
                });
            }
            menu.ShowAsContext();
        }

        private static void TrySuggestParams(TriggerVolume volume, int eventId)
        {
            var types = EventParameterTypeDiscovery.GetSuggestedTypes(eventId);
            if (types.Count == 0) return;

            var table = AssetDatabase.LoadAssetAtPath<PlacedEventTable>(
                "Assets/Resources/Configs/PlacedEventTable.asset");
            var entry = table?.entries.FirstOrDefault(e => e.id == eventId);
            if (entry == null || string.IsNullOrEmpty(entry.defaultParamType)) return;

            var t = Type.GetType(entry.defaultParamType);
            if (t != null && types.Contains(t))
                volume.OnEnter.Parameters = (EventParameterBase)Activator.CreateInstance(t);
        }

        private void BuildEventList()
        {
            _allEvents.Clear();
            var enumValues = Enum.GetValues(typeof(GlobalEventId)).Cast<int>().ToHashSet();

            AddRange("System", 1, 99);
            AddRange("Player", 1000, 1999);
            AddRange("Input", 2000, 2999);
            AddRange("Weapon", 3000, 3999);
            AddRange("Enemy", 4000, 4999);
            AddRange("System", 5000, 5999);

            var table = AssetDatabase.LoadAssetAtPath<PlacedEventTable>(
                "Assets/Resources/Configs/PlacedEventTable.asset");
            if (table != null)
            {
                foreach (var entry in table.entries.OrderBy(e => e.id))
                {
                    if (enumValues.Contains(entry.id)) continue;
                    _allEvents.Add(new SearchableEventPopup.EventEntry
                    {
                        id = entry.id,
                        label = $"[{entry.id}] {entry.displayName} ({entry.name})",
                        category = "Placed",
                    });
                }
            }
        }

        private void AddRange(string category, int min, int max)
        {
            foreach (var val in Enum.GetValues(typeof(GlobalEventId)).Cast<int>().OrderBy(v => v))
            {
                if (val < min || val > max) continue;
                var name = Enum.GetName(typeof(GlobalEventId), val);
                _allEvents.Add(new SearchableEventPopup.EventEntry
                {
                    id = val,
                    label = $"[{val}] {name}",
                    category = category,
                });
            }
        }
    }
}
#endif
