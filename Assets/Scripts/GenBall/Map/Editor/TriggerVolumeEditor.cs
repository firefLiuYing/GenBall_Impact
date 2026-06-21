#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GenBall.Map.Editor
{
    [CustomEditor(typeof(TriggerVolume))]
    public class TriggerVolumeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Trigger name
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("triggerName"),
                new GUIContent("Name"));
            EditorGUILayout.Space();

            // ── Events ──
            EditorGUILayout.LabelField("On Enter Events", EditorStyles.boldLabel);
            var onEnterProp = serializedObject.FindProperty("onEnter");
            EditorGUILayout.PropertyField(onEnterProp, new GUIContent("Events"), true);
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
    }
}
#endif
