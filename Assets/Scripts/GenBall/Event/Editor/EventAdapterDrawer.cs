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
            var eventIdProp = property.FindPropertyRelative("eventId");
            var paramsProp = property.FindPropertyRelative("parameters");
            if (eventIdProp == null || paramsProp == null) return;

            EditorGUI.BeginProperty(position, label, property);

            var y = position.y;
            var lineH = EditorGUIUtility.singleLineHeight;

            // ── Event ID dropdown ──
            var idRect = new Rect(position.x, y, position.width, lineH);
            DrawEventIdDropdown(idRect, eventIdProp);
            y += lineH + 2;

            // ── Parameters ──
            var hasParams = paramsProp.managedReferenceValue != null;

            var labelRect = new Rect(position.x, y, 120, lineH);
            var valueRect = new Rect(position.x + 120, y, position.width - 180, lineH);
            var clearRect = new Rect(position.x + position.width - 50, y, 50, lineH);

            EditorGUI.LabelField(labelRect, "Parameters");
            if (hasParams)
            {
                EditorGUI.LabelField(valueRect, paramsProp.managedReferenceValue.GetType().Name, EditorStyles.miniLabel);
                if (GUI.Button(clearRect, "Clear"))
                {
                    paramsProp.managedReferenceValue = null;
                    paramsProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.LabelField(valueRect, "(none)", EditorStyles.miniLabel);
            }

            y += lineH + 2;

            if (hasParams)
            {
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;
                var detailRect = new Rect(position.x, y, position.width,
                    EditorGUI.GetPropertyHeight(paramsProp, true));
                EditorGUI.PropertyField(detailRect, paramsProp, GUIContent.none, true);
                y += detailRect.height + 2;
                EditorGUI.indentLevel = indent;
            }
            else if (eventIdProp.intValue != 0)
            {
                var suggestedTypes = EventParameterTypeDiscovery.GetSuggestedTypes(eventIdProp.intValue);
                if (suggestedTypes.Count > 0)
                {
                    var addRect = new Rect(position.x + 120, y - lineH - 1, 120, 20);
                    if (GUI.Button(addRect, "Add Parameter..."))
                    {
                        var menu = new GenericMenu();
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
                    y += 2;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var paramsProp = property.FindPropertyRelative("parameters");
            var hasParams = paramsProp != null && paramsProp.managedReferenceValue != null;

            float height = EditorGUIUtility.singleLineHeight * 2 + 6;
            if (hasParams)
                height += EditorGUI.GetPropertyHeight(paramsProp, true);
            return height;
        }

        private static void DrawEventIdDropdown(Rect totalRect, SerializedProperty eventIdProp)
        {
            BuildEventList();

            var curId = eventIdProp.intValue;
            var curEntry = _allEvents?.FirstOrDefault(e => e.id == curId);
            var currentLabel = curId != 0 && curEntry?.label != null
                ? curEntry.Value.label
                : (curId != 0 ? $"[{curId}] Unknown" : "(none)");

            var labelRect = new Rect(totalRect.x, totalRect.y, EditorGUIUtility.labelWidth, totalRect.height);
            var buttonRect = new Rect(totalRect.x + EditorGUIUtility.labelWidth, totalRect.y,
                totalRect.width - EditorGUIUtility.labelWidth, totalRect.height);

            EditorGUI.LabelField(labelRect, "Event");

            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = curId != 0 ? new Color(0.3f, 0.5f, 0.8f, 0.3f) : Color.clear;
            if (GUI.Button(buttonRect, currentLabel, EditorStyles.popup))
            {
                SearchableEventPopup.Show(buttonRect, _allEvents, curId,
                    selectedId =>
                    {
                        eventIdProp.intValue = selectedId;
                        eventIdProp.serializedObject.ApplyModifiedProperties();
                    });
            }
            GUI.backgroundColor = oldBg;
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
