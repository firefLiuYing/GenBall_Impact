using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Buff;
using GenBall.Event;
using GenBall.Map;
using GenBall.Player;
using UnityEngine;

namespace GenBall.Framework.Config
{
    /// <summary>
    /// 配置管理器，统一加载和提供所有游戏配置
    /// Init 时从 Resources 加载所有 ScriptableObject 配置
    /// </summary>
    public class AppConfigManager : IConfigProvider
    {
        private readonly Dictionary<Type, object> _configs = new();

        public void Init()
        {
            LoadConfig<AppSettingsConfig>("Configs/AppSettingsConfig");
            LoadConfig<PlayerConfig>("Configs/PlayerConfig");
            LoadConfig<BuffModelConfig>("Configs/BuffModelConfig");
            var buffConfig = GetConfig<BuffModelConfig>();
            buffConfig?.Init();

            LoadConfig<BulletConfigCollection>("Configs/BulletConfigCollection");
            var bulletConfig = GetConfig<BulletConfigCollection>();
            bulletConfig?.Init();

            LoadConfig<SceneConfigCollection>("Configs/SceneConfigCollection");
            LoadConfig<PlacedEventTable>("Configs/PlacedEventTable");

            Debug.Log($"[AppConfigManager] Loaded {_configs.Count} configs");
        }

        public void UnInit()
        {
            _configs.Clear();
        }

        public T GetConfig<T>() where T : class
        {
            if (_configs.TryGetValue(typeof(T), out var config))
            {
                return (T)config;
            }
            Debug.LogError($"[AppConfigManager] Config {typeof(T).Name} not found");
            return null;
        }

        private void LoadConfig<T>(string resourcePath) where T : ScriptableObject
        {
            var asset = Resources.Load<T>(resourcePath);
            if (asset != null)
            {
                _configs[typeof(T)] = asset;
                Debug.Log($"[AppConfigManager] Loaded config {typeof(T).Name} from {resourcePath}");
            }
            else
            {
                var fallback = ScriptableObject.CreateInstance<T>();
                _configs[typeof(T)] = fallback;
                Debug.LogWarning($"[AppConfigManager] Config {resourcePath} not found, using default");
            }
        }
    }
}
