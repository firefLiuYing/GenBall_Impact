using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons.Accessory
{
    public class AccessoryModelConfig : ScriptableObject
    {
        [SerializeField] private List<AccessoryModel> models=new();
        private readonly Dictionary<AccessoryId, AccessoryModel> _modelDict=new();

        public void Init()
        {
            _modelDict.Clear();
            foreach (var model in models)
            {
                _modelDict.TryAdd(model.Id, model);
            }
        }

        public AccessoryModel GetModel(AccessoryId id) => _modelDict.GetValueOrDefault(id);
    }

    public static class AccessoryModelConfigProvider
    {
        private const string ConfigPath = "Assets/AssetBundles/Config/AccessoryModelConfig.asset";
        private static AccessoryModelConfig _cachedConfig;
        private static bool _configInitialized=false;
        public static AccessoryModelConfig GetOrCreateConfig()
        {
            if(_cachedConfig!=null)  return _cachedConfig;
            var guids=AssetDatabase.FindAssets("t:AccessoryModelConfig");
            if (guids.Length > 1)
            {
                Debug.LogError("gzp 发现多个AccessoryModelConfig，请只保留一个");
                return null;
            }
            if (guids.Length == 1)
            {
                var path=AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedConfig= AssetDatabase.LoadAssetAtPath<AccessoryModelConfig>(path);
                if (!_configInitialized)
                {
                    _cachedConfig.Init();
                    _configInitialized=true;
                }
                return _cachedConfig;
            }
            var config=ScriptableObject.CreateInstance<AccessoryModelConfig>();
            AssetDatabase.CreateAsset(config,ConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log("gzp 已自动创建AccessoryModelConfig");
            _cachedConfig = config;
            if (!_configInitialized)
            {
                _cachedConfig.Init();
                _configInitialized=true;
            }
            return _cachedConfig;
        }
    }
}