using UnityEngine;

namespace GenBall.Map
{
    [PlaceableCategory("SceneTrigger", "场景触发器", 20)]
    [PlaceablePrefab("Assets/AssetBundles/Common/TriggerObject/Trigger.prefab")]
    public class SceneTriggerConfig : MonoBehaviour, IScenePlaceable
    {
        [SerializeField] private string triggerName = "New Trigger";
        [SerializeField] private string eventName;
        [SerializeField] private float triggerRadius = 2f;
        [SerializeField] private TriggerActivationType activationType;
        [SerializeField, HideInInspector] private int id;

        public int Id { get => id; set => id = value; }
        public string DisplayLabel => triggerName;
        public string Category => "SceneTrigger";
        public Transform Anchor => transform;
        public bool IsDynamic => true;

        public object BakeToConfigData() => new SceneTriggerData
        {
            id = id,
            triggerName = triggerName,
            eventName = eventName,
            position = transform.position,
            radius = triggerRadius,
            activationType = activationType,
        };

        public string Validate()
        {
            if (string.IsNullOrEmpty(eventName)) return "Event name is required.";
            if (triggerRadius <= 0f) return "Trigger radius must be > 0.";
            return null;
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
