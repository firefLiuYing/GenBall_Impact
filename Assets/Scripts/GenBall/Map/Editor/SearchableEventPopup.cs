#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GenBall.Map.Editor
{
    /// <summary>
    /// Searchable dropdown window for selecting an event by ID.
    /// Groups events by category, supports text filtering by ID or name.
    /// Keyboard: ↑↓ navigate, Enter select, Esc close.
    /// </summary>
    public class SearchableEventPopup : EditorWindow
    {
        public struct EventEntry
        {
            public int id;
            public string label;
            public string category;
        }

        private static List<EventEntry> _allEvents;
        private static Action<int> _onSelected;

        private List<EventEntry> _filtered;
        private string _search = "";
        private Vector2 _scroll;
        private int _highlightIndex = -1;

        public static void Show(Rect activatorRect,
            List<EventEntry> events,
            int currentId,
            Action<int> onSelected)
        {
            _allEvents = events;
            _onSelected = onSelected;

            var window = CreateInstance<SearchableEventPopup>();
            window._filtered = events;
            window._highlightIndex = events.FindIndex(e => e.id == currentId);

            window.ShowAsDropDown(activatorRect, new Vector2(380, 420));
        }

        private void OnGUI()
        {
            if (_onSelected == null) { Close(); return; }

            // Search field
            GUI.SetNextControlName("EventSearch");
            EditorGUI.BeginChangeCheck();
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
                ApplyFilter();

            if (UnityEngine.Event.current.type == EventType.Repaint && string.IsNullOrEmpty(_search))
            {
                GUI.FocusControl("EventSearch");
                EditorGUI.FocusTextInControl("EventSearch");
            }

            HandleKeyboard();
            DrawEventList();
        }

        private void DrawEventList()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var grouped = _filtered
                .GroupBy(e => e.category)
                .OrderBy(g => CategoryOrder(g.Key));

            foreach (var group in grouped)
            {
                EditorGUILayout.LabelField(
                    $"{group.Key}  ({group.Count()})",
                    new GUIStyle(EditorStyles.boldLabel)
                    {
                        normal = { textColor = CategoryColor(group.Key) },
                        padding = new RectOffset(4, 0, 4, 2),
                    });

                foreach (var entry in group)
                {
                    var isHighlighted = _highlightIndex >= 0
                        && _highlightIndex < _filtered.Count
                        && _filtered[_highlightIndex].id == entry.id;

                    var oldBg = GUI.backgroundColor;
                    if (isHighlighted)
                        GUI.backgroundColor = new Color(0.25f, 0.45f, 0.75f, 0.6f);

                    if (GUILayout.Button("  " + entry.label, new GUIStyle(GUI.skin.label)
                    {
                        padding = new RectOffset(16, 4, 3, 3),
                        normal = { textColor = isHighlighted ? Color.white : GUI.skin.label.normal.textColor },
                    }, GUILayout.Height(18)))
                    {
                        _onSelected?.Invoke(entry.id);
                        Close();
                    }

                    GUI.backgroundColor = oldBg;
                }
                EditorGUILayout.Space(2);
            }

            if (!_filtered.Any())
                EditorGUILayout.HelpBox("No events match.", MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void HandleKeyboard()
        {
            var e = UnityEngine.Event.current;
            if (e.type != EventType.KeyDown) return;

            switch (e.keyCode)
            {
                case KeyCode.DownArrow:
                    e.Use();
                    if (_filtered.Count > 0)
                        _highlightIndex = Mathf.Min(_highlightIndex + 1, _filtered.Count - 1);
                    ScrollToHighlight();
                    break;
                case KeyCode.UpArrow:
                    e.Use();
                    if (_filtered.Count > 0)
                        _highlightIndex = Mathf.Max(_highlightIndex - 1, 0);
                    ScrollToHighlight();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    e.Use();
                    if (_highlightIndex >= 0 && _highlightIndex < _filtered.Count)
                    {
                        _onSelected?.Invoke(_filtered[_highlightIndex].id);
                        Close();
                    }
                    break;
                case KeyCode.Escape:
                    e.Use();
                    Close();
                    break;
            }
        }

        private void ScrollToHighlight()
        {
            if (_highlightIndex < 0 || _filtered.Count == 0) return;
            float y = 0;
            string curGroup = "";
            for (int i = 0; i < _highlightIndex; i++)
            {
                if (_filtered[i].category != curGroup)
                {
                    y += 22;
                    curGroup = _filtered[i].category;
                }
                y += 20;
            }
            _scroll.y = Mathf.Max(0, y - 100);
        }

        private void ApplyFilter()
        {
            var f = _search.Trim().ToLowerInvariant();
            _filtered = string.IsNullOrEmpty(f)
                ? _allEvents
                : _allEvents.Where(e =>
                    e.label.ToLowerInvariant().Contains(f)
                    || e.id.ToString().Contains(f)).ToList();
            _highlightIndex = _filtered.Count > 0 ? 0 : -1;
        }

        private void OnDestroy()
        {
            _allEvents = null;
            _onSelected = null;
        }

        private static int CategoryOrder(string cat) => cat switch
        {
            "System" => 0, "Player" => 1, "Input" => 2,
            "Weapon" => 3, "Enemy" => 4, "Placed" => 5,
            _ => 99,
        };

        private static Color CategoryColor(string cat) => cat switch
        {
            "System" => new Color(0.7f, 0.7f, 0.7f),
            "Player" => new Color(0.4f, 0.8f, 0.4f),
            "Input" => new Color(0.6f, 0.6f, 1f),
            "Weapon" => new Color(1f, 0.6f, 0.2f),
            "Enemy" => new Color(1f, 0.4f, 0.4f),
            "Placed" => new Color(1f, 0.8f, 0.2f),
            _ => Color.white,
        };
    }
}
#endif
