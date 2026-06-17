using GenBall.Utils.Trigger;
using UnityEngine;

namespace GenBall.Map
{
    [PlaceableCategory("SavePoint", "存档点", 0)]
    public class SavePointConfig : MonoBehaviour, IScenePlaceable
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField,HideInInspector] private int index;
        [SerializeField] private string displayName;
        public Transform PlayerSpawnPoint => playerSpawnPoint ?? transform;
        public string DisplayName => displayName;

        public int Index
        {
            get => index;
            set => index = value;
        }

        int IScenePlaceable.Id
        {
            get => index;
            set => index = value;
        }

        string IScenePlaceable.DisplayLabel => displayName;

        string IScenePlaceable.Category => "SavePoint";

        Transform IScenePlaceable.Anchor => PlayerSpawnPoint;

        bool IScenePlaceable.IsDynamic => false;

        object IScenePlaceable.BakeToConfigData() => new SavePointData
        {
            id = index,
            displayName = displayName,
            position = PlayerSpawnPoint.position,
            rotation = PlayerSpawnPoint.rotation,
        };

        string IScenePlaceable.Validate() =>
            string.IsNullOrEmpty(displayName) ? "Display name is required." : null;
    }
}