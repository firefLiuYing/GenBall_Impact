using System.Collections.Generic;
using GenBall.Event;
using GenBall.Event.Params;
using UnityEngine;

namespace GenBall.Map
{
    /// <summary>
    /// A trigger zone that fires a designer-selected event when an object
    /// on the target layers enters it. Uses OnTriggerEnter via a child
    /// TriggerObject at runtime.
    ///
    /// EventAdapter is embedded as a serialized field — the same pattern
    /// can be used by doors, buttons, pressure plates, and other mechanisms.
    /// </summary>
    [PlaceableCategory("SceneTrigger", "触发区域", 20)]
    [PlaceablePrefab("Assets/AssetBundles/Common/TriggerObject/Trigger.prefab")]
    public class TriggerVolume : MonoBehaviour, IScenePlaceable
    {
        [SerializeField]
        private string triggerName = "New Trigger Volume";

        [SerializeField]
        private EventAdapter onEnter = new();

        [SerializeField]
        private float radius = 2f;

        [SerializeField]
        private LayerMask targetLayers = -1;

        [SerializeField]
        private TriggerBehavior triggerBehavior = TriggerBehavior.Once;

        [SerializeField]
        [Tooltip("Max fire count, only relevant when Behavior = Limited.")]
        private int maxFireCount = 1;

        [SerializeField]
        [Tooltip("Minimum seconds between fires. 0 = no cooldown.")]
        private float cooldownSeconds;

        [SerializeField]
        [Tooltip("Optional spawn point for SpawnEnemy events. World position/rotation resolved at bake time.")]
        private Transform spawnPoint;

        [SerializeField, HideInInspector]
        private int id = -1;

        // ── Public accessors ──

        public string TriggerName
        {
            get => triggerName;
            set => triggerName = value;
        }

        public EventAdapter OnEnter => onEnter;

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        public TriggerBehavior Behavior
        {
            get => triggerBehavior;
            set => triggerBehavior = value;
        }

        public LayerMask TargetLayers
        {
            get => targetLayers;
            set => targetLayers = value;
        }

        public Transform SpawnPoint
        {
            get => spawnPoint;
            set => spawnPoint = value;
        }

        // ── IScenePlaceable ──

        public int Id { get => id; set => id = value; }
        public string DisplayLabel => $"[{id}] {triggerName}";
        public string Category => "SceneTrigger";
        public Transform Anchor => transform;
        public bool IsDynamic => true;

        public object BakeToConfigData()
        {
            var serializedEvents = new List<SerializedTriggerEvent>();
            foreach (var entry in onEnter.Entries)
            {
                // Resolve spawnPoint into SpawnEnemyParams before serialization
                if (spawnPoint != null
                    && entry.parameters != null
                    && entry.parameters is SpawnEnemyParams sep)
                {
                    sep.spawnPosition = spawnPoint.position;
                    sep.spawnRotation = spawnPoint.rotation;
                }

                var paramTypeName = entry.parameters != null
                    ? entry.parameters.GetType().AssemblyQualifiedName
                    : null;
                var serializedParams = entry.parameters != null
                    ? JsonUtility.ToJson(entry.parameters)
                    : null;

                serializedEvents.Add(new SerializedTriggerEvent
                {
                    eventId = entry.eventId,
                    paramTypeName = paramTypeName,
                    serializedParams = serializedParams,
                });
            }

            return new SceneTriggerData
            {
                id = id,
                triggerName = triggerName,
                events = serializedEvents,
                position = transform.position,
                radius = radius,
                layerMask = targetLayers.value,
                activationType = TriggerActivationType.OnEnter,
                triggerMode = (int)TriggerMode.Collision,
                triggerBehavior = (int)triggerBehavior,
                maxFireCount = maxFireCount,
                cooldownSeconds = cooldownSeconds,
            };
        }

        public string Validate()
        {
            var entries = onEnter.Entries;
            if (entries.Count == 0)
                return "At least one event must be configured.";
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].eventId == 0)
                    return $"Event [{i}] must have a valid Event ID (non-zero).";
            }
            if (radius <= 0f)
                return "Radius must be > 0.";
            return null;
        }
    }
}
