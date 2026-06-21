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
        private int id;

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
            // Resolve spawnPoint into SpawnEnemyParams before serialization
            if (spawnPoint != null
                && onEnter.HasParameters
                && onEnter.Parameters is SpawnEnemyParams sep)
            {
                sep.spawnPosition = spawnPoint.position;
                sep.spawnRotation = spawnPoint.rotation;
            }

            var paramTypeName = onEnter.HasParameters
                ? onEnter.Parameters.GetType().AssemblyQualifiedName
                : null;
            var serializedParams = onEnter.HasParameters
                ? JsonUtility.ToJson(onEnter.Parameters)
                : null;

            return new SceneTriggerData
            {
                id = id,
                triggerName = triggerName,
                eventId = onEnter.EventId,
                paramTypeName = paramTypeName,
                serializedParams = serializedParams,
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
            if (onEnter.EventId == 0)
                return "Event ID must be set (non-zero).";
            if (radius <= 0f)
                return "Radius must be > 0.";
            return null;
        }
    }
}
