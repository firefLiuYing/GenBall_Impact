using System;
using GenBall.BattleSystem.Character;
using GenBall.Framework.Config;
using UnityEngine;
using Yueyn.Main;
using Yueyn.Resource;
using Object = UnityEngine.Object;

namespace GenBall.Player
{
    public class PlayerSystemDefault : IPlayerSystem
    {
        public GameObject Player { get; private set; }

        private const string PlayerPrefabPath = "Assets/AssetBundles/Common/Player/Prefab/Player.prefab";

        private Vector3 _defaultSpawnPosition;
        private Quaternion _defaultSpawnRotation;

        public void Init()
        {
            var configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            var config = configProvider?.GetConfig<AppSettingsConfig>();
            _defaultSpawnPosition = config != null ? config.defaultPlayerSpawnPosition : Vector3.zero;
            _defaultSpawnRotation = config != null ? Quaternion.Euler(config.defaultPlayerSpawnRotation) : Quaternion.identity;
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

            var prefab = CResourceManager.Instance.LoadSync<GameObject>(PlayerPrefabPath);
            var go = Object.Instantiate(prefab, position, rotation);
            Player = go;
        }
    }
}
