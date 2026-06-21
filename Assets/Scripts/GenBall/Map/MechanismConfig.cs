using UnityEngine;

namespace GenBall.Map
{
    [PlaceableCategory("Mechanism", "机关/可交互物", 30)]
    public class MechanismConfig : MonoBehaviour, IScenePlaceable
    {
        [SerializeField] private string mechanismName = "New Mechanism";
        [SerializeField] private string mechanismType;
        [SerializeField, HideInInspector] private int id = -1;

        public int Id { get => id; set => id = value; }
        public string DisplayLabel => mechanismName;
        public string Category => "Mechanism";
        public Transform Anchor => transform;
        public bool IsDynamic => false;

        public object BakeToConfigData() => new MechanismData
        {
            id = id,
            mechanismName = mechanismName,
            mechanismType = mechanismType,
            position = transform.position,
            rotation = transform.rotation,
            customDataJson = null,
        };

        public string Validate()
        {
            if (string.IsNullOrEmpty(mechanismType)) return "Mechanism type is required.";
            return null;
        }
    }
}
