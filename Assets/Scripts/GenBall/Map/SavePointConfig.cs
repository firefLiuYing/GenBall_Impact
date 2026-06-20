using GenBall.Utils.Trigger;
using UnityEngine;

namespace GenBall.Map
{
    [PlaceableCategory("SavePoint", "存档点", 0)]
    [PlaceablePrefab("Assets/MapEditorPrefabs/SavePoint/SavePoint.prefab")]
    public class SavePointConfig : MonoBehaviour, IScenePlaceable
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private int index;
        [SerializeField] private string displayName;
        [SerializeField] private string bonfireType = "";     // "" = pure anchor, non-empty = bonfire
        [SerializeField] private bool initiallyActive = true;  // spawn on scene init
        public Transform PlayerSpawnPoint => playerSpawnPoint ?? transform;
        public string DisplayName => displayName;

        public int Index
        {
            get => index;
            set => index = value;
        }

        public string BonfireType => bonfireType;
        public bool InitiallyActive => initiallyActive;

        int IScenePlaceable.Id
        {
            get => index;
            set => index = value;
        }

        string IScenePlaceable.DisplayLabel =>
            string.IsNullOrEmpty(displayName)
                ? $"[{index}] {(string.IsNullOrEmpty(bonfireType) ? "Anchor" : bonfireType)}"
                : $"[{index}] {displayName}";

        string IScenePlaceable.Category => "SavePoint";

        Transform IScenePlaceable.Anchor => PlayerSpawnPoint;

        bool IScenePlaceable.IsDynamic => true;

        object IScenePlaceable.BakeToConfigData() => new SavePointData
        {
            id = index,
            displayName = displayName,
            position = PlayerSpawnPoint.position,
            rotation = PlayerSpawnPoint.rotation,
            bonfireType = bonfireType,
            initiallyActive = initiallyActive,
            bonfirePosition = transform.position,
            bonfireRotation = transform.rotation,
        };

        string IScenePlaceable.Validate()
        {
            // Bonfires require a display name (shown in UI during interaction)
            if (!string.IsNullOrEmpty(bonfireType) && string.IsNullOrEmpty(displayName))
                return "Bonfire requires a display name.";
            return null;
        }
    }
}