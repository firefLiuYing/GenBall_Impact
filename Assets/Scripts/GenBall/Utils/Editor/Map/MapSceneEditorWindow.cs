#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GenBall.Utils.Editor.Map
{
    /// <summary>
    /// Main scene editor window for managing IScenePlaceable objects.
    /// Provides category tree view, property editing, validation, and baking.
    /// </summary>
    public class MapSceneEditorWindow : EditorWindow
    {
        private List<IScenePlaceable> _placeables = new();
        private Dictionary<string, List<IScenePlaceable>> _grouped = new();
        private Vector2 _treeScroll;
        private Vector2 _propScroll;
        private IScenePlaceable _selected;
        private UnityEditor.Editor _selectedEditor;
        private string _validationResult;
        private bool _validationPassed;

        [MenuItem("Tools/Map/Map Scene Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<MapSceneEditorWindow>("Map Scene Editor");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshSceneData();
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            DestroySelectedEditor();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode) => RefreshSceneData();

        private void OnFocus()
        {
            // Refresh when window gains focus (scene might have changed)
            var currentScene = SceneManager.GetActiveScene();
            if (_placeables.Count == 0 || _placeables.Any(p => p?.Anchor == null))
                RefreshSceneData();
        }

        private void RefreshSceneData()
        {
            DestroySelectedEditor();
            _selected = null;
            _placeables.Clear();
            _grouped.Clear();

            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded) return;

            // Find all root GameObjects and collect IScenePlaceable from them and children
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var components = root.GetComponentsInChildren<MonoBehaviour>(true)
                    .OfType<IScenePlaceable>();
                _placeables.AddRange(components);
            }

            _grouped = _placeables
                .GroupBy(p => p.Category)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => p.DisplayLabel).ToList());

            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();

            // Left panel: category tree
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.45f));
            DrawCategoryTree();
            EditorGUILayout.EndVertical();

            // Separator
            EditorGUILayout.BeginVertical(GUILayout.Width(2));
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(2), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, Color.gray);
            EditorGUILayout.EndVertical();

            // Right panel: property inspector
            EditorGUILayout.BeginVertical();
            DrawPropertyPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // Status bar
            DrawStatusBar();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var scene = SceneManager.GetActiveScene();
            GUILayout.Label($"Scene: {scene.name}", EditorStyles.miniLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshSceneData();
            }

            if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RunValidation();
            }

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Bake Current Scene", EditorStyles.toolbarButton, GUILayout.Width(130)))
            {
                BakingPipeline.BakeCurrentScene(_placeables);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategoryTree()
        {
            GUILayout.Label("Placeables", EditorStyles.boldLabel);
            _treeScroll = EditorGUILayout.BeginScrollView(_treeScroll);

            foreach (var kvp in _grouped)
            {
                var category = kvp.Key;
                var items = kvp.Value;
                var displayName = GetCategoryDisplayName(category);

                // Get the PlaceableCategoryAttribute for the display name
                var typeInfo = PlaceableTypeDiscovery.DiscoverAll()
                    .FirstOrDefault(t => t.CategoryAttribute.Category == category);
                if (typeInfo != null)
                    displayName = typeInfo.CategoryAttribute.DisplayName;

                var foldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold,
                };
                var isExpanded = EditorPrefs.GetBool($"MapSceneEditor_{category}", true);
                isExpanded = EditorGUILayout.Foldout(isExpanded,
                    $"{displayName} ({items.Count})", true, foldoutStyle);
                EditorPrefs.SetBool($"MapSceneEditor_{category}", isExpanded);

                if (isExpanded)
                {
                    EditorGUI.indentLevel++;
                    foreach (var item in items)
                    {
                        var isSelected = _selected == item;
                        GUI.backgroundColor = isSelected ? new Color(0.4f, 0.6f, 1f) : Color.white;
                        if (GUILayout.Button(item.DisplayLabel, EditorStyles.label))
                        {
                            SelectPlaceable(item);
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndScrollView();

            // Add new placeable context menu
            if (GUILayout.Button("+ Add New Placeable", GUILayout.Height(25)))
            {
                ShowAddPlaceableMenu();
            }
        }

        private void DrawPropertyPanel()
        {
            GUILayout.Label("Properties", EditorStyles.boldLabel);

            if (_selected == null)
            {
                EditorGUILayout.HelpBox(
                    "Select a placeable from the left panel to edit its properties.\n\n" +
                    "Use the toolbar buttons to validate all placeables or bake to config.",
                    MessageType.Info);
                return;
            }

            var mb = _selected as MonoBehaviour;
            if (mb == null)
            {
                EditorGUILayout.HelpBox("Selected object is not a MonoBehaviour.", MessageType.Error);
                return;
            }

            _propScroll = EditorGUILayout.BeginScrollView(_propScroll);

            // Frame Select button
            if (GUILayout.Button("Frame Select in Scene View", GUILayout.Height(22)))
            {
                Selection.activeGameObject = mb.gameObject;
                SceneView.lastActiveSceneView?.FrameSelected();
            }

            // Delete button
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Remove Placeable", GUILayout.Height(22)))
            {
                if (EditorUtility.DisplayDialog("Remove Placeable",
                    $"Delete '{_selected.DisplayLabel}'?", "Delete", "Cancel"))
                {
                    DestroySelectedEditor();
                    Undo.DestroyObjectImmediate(mb.gameObject);
                    RefreshSceneData();
                    return;
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();

            // Draw inspector for the selected object
            if (_selectedEditor == null || _selectedEditor.target != mb)
            {
                DestroySelectedEditor();
                _selectedEditor = UnityEditor.Editor.CreateEditor(mb);
            }

            if (_selectedEditor != null)
            {
                _selectedEditor.OnInspectorGUI();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label($"{_placeables.Count} placeables in current scene", EditorStyles.miniLabel);

            if (!string.IsNullOrEmpty(_validationResult))
            {
                var color = _validationPassed ? Color.green : Color.red;
                var oldColor = GUI.color;
                GUI.color = color;
                GUILayout.FlexibleSpace();
                GUILayout.Label(_validationResult, EditorStyles.miniLabel);
                GUI.color = oldColor;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SelectPlaceable(IScenePlaceable placeable)
        {
            _selected = placeable;
            DestroySelectedEditor();
            if (placeable is MonoBehaviour mb)
            {
                Selection.activeGameObject = mb.gameObject;
            }
            Repaint();
        }

        private void RunValidation()
        {
            if (BakingPipeline.RunValidation(_placeables, out var errors))
            {
                _validationPassed = true;
                _validationResult = $"All {_placeables.Count} placeables valid";
            }
            else
            {
                _validationPassed = false;
                _validationResult = $"{errors.Count} error(s) found";
                var errorMsg = string.Join("\n", errors);
                EditorUtility.DisplayDialog("Validation Results",
                    $"Found {errors.Count} error(s):\n\n{errorMsg}", "OK");
            }
            Repaint();
        }

        private void ShowAddPlaceableMenu()
        {
            var menu = new GenericMenu();
            var types = PlaceableTypeDiscovery.DiscoverAll();

            // Group by category for cascading sub-menus
            var grouped = types.GroupBy(t => t.CategoryAttribute.Category)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in grouped)
            {
                var typesInGroup = group.ToList();
                var categoryLabel = group.First().CategoryAttribute.DisplayName;

                if (typesInGroup.Count == 1)
                {
                    // Single type: show directly under root
                    var info = typesInGroup[0];
                    AddMenuItem(menu, categoryLabel, info);
                }
                else
                {
                    // Multiple types: show as cascading sub-menu
                    foreach (var info in typesInGroup)
                    {
                        var subLabel = $"{categoryLabel}/{info.CategoryAttribute.DisplayName}";
                        AddMenuItem(menu, subLabel, info);
                    }
                }
            }

            menu.ShowAsContext();
        }

        private void AddMenuItem(GenericMenu menu, string label, PlaceableTypeDiscovery.PlaceableTypeInfo info)
        {
            if (info.PrefabAttribute != null)
            {
                var prefabPath = info.PrefabAttribute.PrefabPath;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    menu.AddItem(new GUIContent(label), false, () => InstantiatePlaceablePrefab(prefabPath));
                }
                else
                {
                    // Prefab not found: create empty GO with the component as fallback
                    var type = info.Type;
                    menu.AddItem(new GUIContent(label + " (fallback)"), false, () => CreatePlaceableFromType(type));
                }
            }
            else
            {
                // No prefab: create empty GO with the component
                var type = info.Type;
                menu.AddItem(new GUIContent(label), false, () => CreatePlaceableFromType(type));
            }
        }

        /// <summary>
        /// Creates an empty GameObject with the given placeable component type.
        /// Fallback when no prefab is registered for a placeable type.
        /// </summary>
        private void CreatePlaceableFromType(System.Type type)
        {
            var sceneView = SceneView.lastActiveSceneView;
            Vector3 spawnPos = Vector3.zero;
            if (sceneView?.camera != null)
            {
                spawnPos = sceneView.camera.transform.position +
                    sceneView.camera.transform.forward * 5f;
            }

            var go = new GameObject(type.Name);
            go.transform.position = spawnPos;
            go.AddComponent(type);

            Undo.RegisterCreatedObjectUndo(go, "Add Placeable");
            Selection.activeGameObject = go;

            RefreshSceneData();

            var placeable = go.GetComponent<IScenePlaceable>();
            if (placeable != null)
            {
                _selected = placeable;
                Repaint();
            }
        }

        private void InstantiatePlaceablePrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[MapSceneEditor] Prefab not found at: {prefabPath}");
                return;
            }

            var sceneView = SceneView.lastActiveSceneView;
            Vector3 spawnPos = Vector3.zero;
            if (sceneView?.camera != null)
            {
                // Spawn 5 units in front of the scene view camera
                spawnPos = sceneView.camera.transform.position +
                    sceneView.camera.transform.forward * 5f;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = spawnPos;
            instance.name = prefab.name;

            Undo.RegisterCreatedObjectUndo(instance, "Add Placeable");
            Selection.activeGameObject = instance;

            // Refresh to pick up the new instance
            RefreshSceneData();

            // Select the new placeable
            var placeable = instance.GetComponent<IScenePlaceable>();
            if (placeable != null)
            {
                _selected = placeable;
                Repaint();
            }
        }

        private string GetCategoryDisplayName(string category)
        {
            var typeInfo = PlaceableTypeDiscovery.DiscoverAll()
                .FirstOrDefault(t => t.CategoryAttribute.Category == category);
            return typeInfo?.CategoryAttribute.DisplayName ?? category;
        }

        private void DestroySelectedEditor()
        {
            if (_selectedEditor != null)
            {
                DestroyImmediate(_selectedEditor);
                _selectedEditor = null;
            }
        }
    }
}
#endif
