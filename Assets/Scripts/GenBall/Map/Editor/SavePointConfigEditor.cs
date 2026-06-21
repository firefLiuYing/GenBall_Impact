#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GenBall.Map
{
    [CustomEditor(typeof(SavePointConfig))]
    public class SavePointConfigEditor : UnityEditor.Editor
    {
        private int _bonfireTypeIndex;

        public override void OnInspectorGUI()
        {
            var config = (SavePointConfig)target;
            serializedObject.Update();

            // bonfireType dropdown
            DrawBonfireTypeDropdown(config);

            // initiallyActive
            var initiallyActiveProp = serializedObject.FindProperty("initiallyActive");
            if (initiallyActiveProp != null)
                EditorGUILayout.PropertyField(initiallyActiveProp, new GUIContent("Initially Active"));

            // playerSpawnPoint
            var spawnPointProp = serializedObject.FindProperty("playerSpawnPoint");
            if (spawnPointProp != null)
                EditorGUILayout.PropertyField(spawnPointProp, new GUIContent("Player Spawn Point"));

            // displayName
            var displayNameProp = serializedObject.FindProperty("displayName");
            if (displayNameProp != null)
                EditorGUILayout.PropertyField(displayNameProp, new GUIContent("Display Name"));

            // index — read-only (assigned by baking pipeline)
            var indexProp = serializedObject.FindProperty("index");
            if (indexProp != null)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(indexProp, new GUIContent("ID (baked)"));
                GUI.enabled = true;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBonfireTypeDropdown(SavePointConfig config)
        {
            var types = new System.Collections.Generic.List<string>();
            // We read from BonfirePrefabRegistry via serialized property approach
            // Use reflection to access the private serialized field for display

            // Get the serialized property for bonfireType
            var bonfireTypeProp = serializedObject.FindProperty("bonfireType");
            if (bonfireTypeProp == null) return;

            // Build the dropdown options list
            var displayOptions = new System.Collections.Generic.List<string> { "(None — pure anchor)" };
            var typeValues = new System.Collections.Generic.List<string> { "" };

            foreach (var t in BonfirePrefabRegistry.RegisteredTypes)
            {
                displayOptions.Add(t);
                typeValues.Add(t);
            }

            // Find current index
            var currentValue = bonfireTypeProp.stringValue;
            var currentIndex = typeValues.IndexOf(currentValue);
            if (currentIndex < 0) currentIndex = 0;

            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup("Bonfire Type", currentIndex, displayOptions.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                bonfireTypeProp.stringValue = typeValues[newIndex];
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif
