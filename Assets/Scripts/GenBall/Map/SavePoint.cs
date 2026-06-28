using System.Collections.Generic;
using GenBall.CombatState;
using GenBall.Event;
using GenBall.Interact;
using GenBall.Procedure;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Map
{
    public class SavePoint : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private List<EventAdapter> _interactEvents = new();

        [SerializeField]
        private string _displayName;

        private ICombatStateSystem _combatStateSystem;
        private ISceneStateSystem _sceneStateSystem;
        private IGameManagerSystem _gameManager;

        /// <summary>Set by SpawnBonfires during scene initialization.</summary>
        public int SavePointIndex { get; private set; } = -1;

        /// <summary>
        /// Whether this save point has been unlocked.
        /// Set by SpawnBonfires (from save data) or Interact() (first-time unlock).
        /// </summary>
        public bool IsUnlocked { get; private set; }

        /// <summary>
        /// Injected by SpawnBonfires during scene initialization.
        /// </summary>
        public void SetConfig(string displayName, int index, bool isUnlocked)
        {
            _displayName = displayName;
            SavePointIndex = index;
            IsUnlocked = isUnlocked;
            ApplyVisualState();
        }

        private void Awake()
        {
            _combatStateSystem = SystemRepository.Instance.GetSystem<ICombatStateSystem>();
            _sceneStateSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            _gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
        }

        public string OperationDescription => _displayName;
        public bool CanInteract => _combatStateSystem != null && !_combatStateSystem.IsInCombat;

        public void Interact()
        {
            if (!IsUnlocked)
            {
                Unlock();
            }
            else
            {
                OpenBonfireUI();
            }
        }

        private void Unlock()
        {
            IsUnlocked = true;
            ApplyVisualState();

            // Register in memory
            var sceneName = gameObject.scene.name;
            _sceneStateSystem?.UnlockSavePoint(sceneName, SavePointIndex);

            // Persist incrementally
            PersistUnlockState(sceneName);

            // Fire configured events (e.g., effects, sounds)
            foreach (var evt in _interactEvents)
            {
                evt?.Fire();
            }

            Debug.Log($"[SavePoint] Unlocked: {_displayName} (index={SavePointIndex})");
        }

        private async void PersistUnlockState(string sceneName)
        {
            if (_gameManager == null) return;

            var mapProvider = _gameManager.GetProvider("Map") as MapSaveDataProvider;
            if (mapProvider == null) return;

            // Ensure the scene entry exists in provider's runtime data
            var sceneData = mapProvider.RuntimeData.unlockedScenes
                .Find(s => s.sceneName == sceneName);
            if (sceneData == null)
            {
                sceneData = new SceneSaveData
                {
                    sceneName = sceneName,
                    unlockedSavePoints = new List<int>(),
                    killedEnemyUnits = new List<int>(),
                };
                mapProvider.RuntimeData.unlockedScenes.Add(sceneData);
            }

            if (!sceneData.unlockedSavePoints.Contains(SavePointIndex))
                sceneData.unlockedSavePoints.Add(SavePointIndex);

            // Collect updated state and persist
            var json = mapProvider.CollectSaveData();
            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Map.UnlockedScenes, json }
            };
            await _gameManager.UpdateSaveFields("Map", fields);
        }

        private void OpenBonfireUI()
        {
            Debug.Log($"[SavePoint] Open bonfire UI: {_displayName}");
            // TODO: D-3e — open holographic UI (BonfireForm)
            foreach (var evt in _interactEvents)
            {
                evt?.Fire();
            }
        }

        private void ApplyVisualState()
        {
            // TODO: swap materials/animations for locked vs unlocked
            // Placeholder implementation — subclasses or future VFX integration
        }

        public void OnFocused()
        {
            Debug.Log($"[SavePoint] Focused: {_displayName}");
        }

        public void OnUnfocused()
        {
            Debug.Log($"[SavePoint] Unfocused: {_displayName}");
        }
    }
}
