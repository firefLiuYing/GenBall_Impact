#if UNITY_EDITOR
using System.Linq;
using GenBall.Map;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GenBall.Utils.Editor.Map
{
    [CustomPropertyDrawer(typeof(SavePointReferenceAttribute))]
    public class SavePointReferenceDrawer : PropertyDrawer
    {
        private const string ConfigAssetPath = "Assets/Resources/Configs/SceneConfigCollection.asset";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.HelpBox(position, "SavePointReference attribute must be used on int fields.", MessageType.Error);
                return;
            }

            var attr = (SavePointReferenceAttribute)attribute;
            var config = AssetDatabase.LoadAssetAtPath<SceneConfigCollection>(ConfigAssetPath);

            // Build options list
            var displayOptions = new System.Collections.Generic.List<string>();
            var idOptions = new System.Collections.Generic.List<int>();

            if (attr.CrossScene)
            {
                // All scenes
                if (config != null)
                {
                    foreach (var entry in config.scenes)
                    {
                        foreach (var sp in entry.savePoints)
                        {
                            displayOptions.Add($"{entry.displayName} / [{sp.id}] {sp.displayName}");
                            idOptions.Add(sp.id);
                        }
                    }
                }
            }
            else
            {
                // Current scene only
                var currentSceneName = SceneManager.GetActiveScene().name;
                if (config != null)
                {
                    var entry = config.scenes.FirstOrDefault(s => s.sceneName == currentSceneName);
                    if (entry != null)
                    {
                        foreach (var sp in entry.savePoints)
                        {
                            displayOptions.Add($"[{sp.id}] {sp.displayName}");
                            idOptions.Add(sp.id);
                        }
                    }
                }
            }

            // Handle empty config (not baked yet)
            if (displayOptions.Count == 0)
            {
                displayOptions.Add(config == null ? "(No SceneConfigCollection)" : "(Not baked)");
                idOptions.Add(-1);
            }

            // Find current index
            var currentId = property.intValue;
            var currentIndex = idOptions.IndexOf(currentId);
            if (currentIndex < 0) currentIndex = 0;

            // Layout: dropdown takes most of the width, optional ping button
            var dropdownRect = attr.CrossScene
                ? position
                : new Rect(position.x, position.y, position.width - 40, position.height);
            var buttonRect = new Rect(position.x + position.width - 35, position.y, 35, position.height);

            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, displayOptions.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = idOptions[newIndex];
            }

            // Ping button (scene-internal only)
            if (!attr.CrossScene && idOptions[newIndex] >= 0)
            {
                if (GUI.Button(buttonRect, "\u25ce"))
                {
                    PingSavePoint(idOptions[newIndex]);
                }
            }

            // Warn if stored ID doesn't match any option
            if (currentId >= 0 && !idOptions.Contains(currentId))
            {
                var warningRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2,
                    position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(warningRect,
                    $"SavePoint id={currentId} not found in current config. Value preserved.",
                    MessageType.Warning);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = (SavePointReferenceAttribute)attribute;
            var config = AssetDatabase.LoadAssetAtPath<SceneConfigCollection>(ConfigAssetPath);
            var currentId = property.intValue;

            // Check if we need the warning line
            bool needsWarning = false;
            if (currentId >= 0 && config != null)
            {
                if (!attr.CrossScene)
                {
                    var currentSceneName = SceneManager.GetActiveScene().name;
                    var entry = config.scenes.FirstOrDefault(s => s.sceneName == currentSceneName);
                    needsWarning = entry == null || !entry.savePoints.Exists(sp => sp.id == currentId);
                }
            }

            var baseHeight = EditorGUIUtility.singleLineHeight;
            return needsWarning ? baseHeight * 2 + 4 : baseHeight;
        }

        private static void PingSavePoint(int id)
        {
            var configs = Object.FindObjectsOfType<SavePointConfig>();
            foreach (var sp in configs)
            {
                if (sp.Index == id)
                {
                    Selection.activeGameObject = sp.gameObject;
                    if (SceneView.lastActiveSceneView != null)
                        SceneView.lastActiveSceneView.FrameSelected();
                    return;
                }
            }
            Debug.LogWarning($"[SavePointReference] SavePoint id={id} not found in current scene.");
        }
    }
}
#endif
