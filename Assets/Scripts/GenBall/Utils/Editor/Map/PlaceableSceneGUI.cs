#if UNITY_EDITOR
using GenBall.Map;
using GenBall.Map.EnemyUnitConfig;
using UnityEditor;
using UnityEngine;

namespace GenBall.Utils.Editor.Map
{
    /// <summary>
    /// Draws Scene View gizmos and handles for GameObjects with IScenePlaceable components.
    /// </summary>
    public static class PlaceableSceneGUI
    {
        private static readonly Color EnemyColor = new(1f, 0.3f, 0.3f, 0.8f);
        private static readonly Color SavePointColor = new(0.3f, 0.8f, 0.3f, 0.8f);
        private static readonly Color TriggerColor = new(1f, 0.8f, 0.2f, 0.8f);
        private static readonly Color MechanismColor = new(0.3f, 0.6f, 1f, 0.8f);
        private static readonly Color DefaultColor = Color.gray;

        /// <summary>
        /// Draw gizmos for a single placeable. Call from OnDrawGizmos in a custom editor or from
        /// a scene GUI callback.
        /// </summary>
        public static void DrawGizmo(IScenePlaceable placeable, bool isSelected)
        {
            if (placeable?.Anchor == null) return;

            var color = GetCategoryColor(placeable.Category);
            var pos = placeable.Anchor.position;

            // Category icon sphere
            Gizmos.color = color;
            Gizmos.DrawSphere(pos, 0.15f);

            // Direction indicator
            var forward = placeable.Anchor.forward;
            Gizmos.DrawLine(pos, pos + forward * 0.5f);

            // Label
            var style = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = isSelected ? Color.white : color },
                fontSize = 11,
                fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal,
            };
            Handles.Label(pos + Vector3.up * 0.4f,
                $"[{placeable.Category}] {placeable.DisplayLabel}", style);

            // Category-specific gizmos
            switch (placeable)
            {
                case EnemyUnitConfigBase enemy:
                    if (enemy.PatrolRadius > 0f)
                    {
                        Gizmos.color = new Color(color.r, color.g, color.b, 0.15f);
                        Gizmos.DrawWireSphere(pos, enemy.PatrolRadius);
                    }
                    if (enemy.DetectRadius > 0f)
                    {
                        Gizmos.color = new Color(color.r, color.g, color.b, 0.1f);
                        Gizmos.DrawWireSphere(pos, enemy.DetectRadius);
                    }
                    break;

                case SceneTriggerConfig trigger:
                    // draw dynamic access via reflection to avoid coupling
                    var radiusField = trigger.GetType().GetField("triggerRadius",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (radiusField != null)
                    {
                        var radius = (float)radiusField.GetValue(trigger);
                        if (radius > 0f)
                        {
                            Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
                            Gizmos.DrawWireSphere(pos, radius);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Draw a position handle for the placeable. Call from OnSceneGUI in a custom editor.
        /// </summary>
        public static void DrawPositionHandle(IScenePlaceable placeable)
        {
            if (placeable?.Anchor == null) return;

            EditorGUI.BeginChangeCheck();
            var newPos = Handles.PositionHandle(placeable.Anchor.position, placeable.Anchor.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(placeable.Anchor, "Move Placeable");
                placeable.Anchor.position = newPos;
            }
        }

        private static Color GetCategoryColor(string category)
        {
            return category switch
            {
                "Enemy" => EnemyColor,
                "SavePoint" => SavePointColor,
                "SceneTrigger" => TriggerColor,
                "Mechanism" => MechanismColor,
                _ => DefaultColor,
            };
        }
    }
}
#endif
