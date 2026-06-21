#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Map.Editor;
using UnityEditor;
using UnityEngine;

namespace GenBall.Event.Editor
{
    [CustomPropertyDrawer(typeof(EventAdapter))]
    public class EventAdapterDrawer : PropertyDrawer
    {
        private static List<SearchableEventPopup.EventEntry> _allEvents;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var entriesProp = property.FindPropertyRelative("_entries");
            if (entriesProp == null) return;

            EditorGUI.BeginProperty(position, label, property);

            var y = position.y;
            var lineH = EditorGUIUtility.singleLineHeight;
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw each entry as a compact row
            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var entryProp = entriesProp.GetArrayElementAtIndex(i);
                var eventIdProp = entryProp.FindPropertyRelative("eventId");
                var paramsProp = entryProp.FindPropertyRelative("parameters");

                y += DrawEntryRow(new Rect(position.x, y, position.width, lineH),
                    i, entriesProp.arraySize, eventIdProp, paramsProp, entriesProp);
            }

            // [+ Add Event] button
            var addRect = new Rect(position.x, y, 120, lineH);
            if (GUI.Button(addRect, "+ Add Event"))
            {
                entriesProp.arraySize++;
                entriesProp.serializedObject.ApplyModifiedProperties();
            }

            y += lineH + 2;

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var entriesProp = property.FindPropertyRelative("_entries");
            if (entriesProp == null) return EditorGUIUtility.singleLineHeight;

            var lineH = EditorGUIUtility.singleLineHeight;
            var totalH = entriesProp.arraySize * (lineH + 2);

            // Check for expanded parameters
            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var paramsProp = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("parameters");
                if (paramsProp != null && paramsProp.managedReferenceValue != null)
                    totalH += EditorGUI.GetPropertyHeight(paramsProp, true) + 2;
            }

            totalH += lineH + 2; // + Add Event button
            return totalH;
        }

        private static float DrawEntryRow(Rect rowRect, int index, int total,
            SerializedProperty eventIdProp, SerializedProperty paramsProp,
            SerializedProperty entriesProp)
        {
            var lineH = EditorGUIUtility.singleLineHeight;
            var hasParams = paramsProp.managedReferenceValue != null;

            // Layout: [label "Event"] [dropdown button] [param info or "Add Param"] [-]
            var labelW = 45f;
            var removeW = 20f;
            var paramW = hasParams ? 140f : 100f;
            var dropW = rowRect.width - labelW - paramW - removeW - 4;

            var labelRect = new Rect(rowRect.x, rowRect.y, labelW, lineH);
            var dropRect = new Rect(rowRect.x + labelW, rowRect.y, dropW, lineH);
            var paramRect = new Rect(dropRect.xMax + 2, rowRect.y, paramW, lineH);

            EditorGUI.LabelField(labelRect, "Event");

            // Event ID dropdown
            BuildEventList();
            var curId = eventIdProp.intValue;
            var curEntry = _allEvents?.FirstOrDefault(e => e.id == curId);
            var currentLabel = curId != 0 && curEntry?.label != null
                ? curEntry.Value.label
                : (curId != 0 ? $"[{curId}] Unknown" : "(none)");

            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = curId != 0 ? new Color(0.3f, 0.5f, 0.8f, 0.3f) : Color.clear;
            if (GUI.Button(dropRect, currentLabel, EditorStyles.popup))
            {
                SearchableEventPopup.Show(dropRect, _allEvents, curId,
                    selectedId =>
                    {
                        eventIdProp.intValue = selectedId;
                        eventIdProp.serializedObject.ApplyModifiedProperties();
                    });
            }
            GUI.backgroundColor = oldBg;

            // Parameters: show type name or Add Parameter button
            if (hasParams)
            {
                var typeLabelRect = new Rect(paramRect.x, paramRect.y, paramRect.width - 22, lineH);
                var clearRect = new Rect(paramRect.xMax - 20, paramRect.y, 20, lineH);
                EditorGUI.LabelField(typeLabelRect, paramsProp.managedReferenceValue.GetType().Name,
                    EditorStyles.miniLabel);
                if (GUI.Button(clearRect, "x", EditorStyles.miniButton))
                {
                    paramsProp.managedReferenceValue = null;
                    paramsProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else if (eventIdProp.intValue != 0)
            {
                var suggestedTypes = EventParameterTypeDiscovery.GetSuggestedTypes(eventIdProp.intValue);
                if (suggestedTypes.Count > 0)
                {
                    if (GUI.Button(paramRect, "Add Param...", EditorStyles.miniButton))
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("(None)"), true, () =>
                        {
                            paramsProp.managedReferenceValue = null;
                            paramsProp.serializedObject.ApplyModifiedProperties();
                        });
                        menu.AddSeparator("");
                        foreach (var t in suggestedTypes)
                        {
                            var captured = t;
                            menu.AddItem(new GUIContent(t.Name), false, () =>
                            {
                                paramsProp.managedReferenceValue = Activator.CreateInstance(captured);
                                paramsProp.serializedObject.ApplyModifiedProperties();
                            });
                        }
                        menu.ShowAsContext();
                    }
                }
            }

            // Remove button
            var removeRect = new Rect(rowRect.xMax - 20, rowRect.y, 20, lineH);
            if (GUI.Button(removeRect, "-", EditorStyles.miniButton))
            {
                entriesProp.DeleteArrayElementAtIndex(index);
                entriesProp.serializedObject.ApplyModifiedProperties();
            }

            var extraH = 0f;
            if (hasParams)
            {
                var detailRect = new Rect(rowRect.x, rowRect.y + lineH + 2,
                    rowRect.width, EditorGUI.GetPropertyHeight(paramsProp, true));
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(detailRect, paramsProp, GUIContent.none, true);
                EditorGUI.indentLevel--;
                extraH = detailRect.height + 2;
            }

            return lineH + 2 + extraH;
        }

        private static void BuildEventList()
        {
            if (_allEvents != null) return;
            _allEvents = new List<SearchableEventPopup.EventEntry>();

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

        private static void AddRange(string category, int min, int max)
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
