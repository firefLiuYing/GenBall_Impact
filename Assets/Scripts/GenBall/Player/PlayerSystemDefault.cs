using System;
using GenBall.BattleSystem.Character;
using GenBall.Framework.Config;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player
{
    public class PlayerSystemDefault : IPlayerSystem
    {
        public GameObject Player { get; private set; }

        private Vector3 _defaultSpawnPosition;
        private Quaternion _defaultSpawnRotation;

        public void Init()
        {
            var configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            var config = configProvider?.GetConfig<AppSettingsConfig>();
            _defaultSpawnPosition = config != null ? config.defaultPlayerSpawnPosition : Vector3.zero;
            _defaultSpawnRotation = config != null ? Quaternion.Euler(config.defaultPlayerSpawnRotation) : Quaternion.identity;

            try
            {
                GameEntry.CharacterCreator.AddPrefab<CharacterState>(
                    "Player",
                    "Assets/AssetBundles/Common/Player/Prefab/Player.prefab");
            }
            catch (NullReferenceException)
            {
                // GameEntry not available in EditMode tests
            }
        }

        public void UnInit()
        {
            Player = null;
        }

        public void CreatePlayer()
        {
            CreatePlayer(_defaultSpawnPosition, _defaultSpawnRotation);
        }

        public void CreatePlayer(Vector3 position, Quaternion rotation)
        {
            if (Player != null)
                throw new Exception("Current scene already has a Player");

            var player = GameEntry.CharacterCreator.CreateEntity<CharacterState>(
                "Player", position, rotation, null);
            Player = player.gameObject;
        }
    }
}
