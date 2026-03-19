using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons.Accessory
{
    public class EvolutionConfig:ScriptableObject
    {
        public List<int> loadOfLevel=new();
    }
    public static class EvolutionConfigProvider
    {
        private const string ConfigPath = "Assets/AssetBundles/Config/EvolutionConfig.asset";
        private static EvolutionConfig _cachedConfig;
        public static EvolutionConfig GetOrCreateConfig()
        {
            if(_cachedConfig!=null)  return _cachedConfig;
            var guids=AssetDatabase.FindAssets("t:EvolutionConfig");
            if (guids.Length > 1)
            {
                Debug.LogError("gzp 发现多个EvolutionConfig，请只保留一个");
                return null;
            }
            if (guids.Length == 1)
            {
                var path=AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedConfig= AssetDatabase.LoadAssetAtPath<EvolutionConfig>(path);
                return _cachedConfig;
            }
            var config=ScriptableObject.CreateInstance<EvolutionConfig>();
            AssetDatabase.CreateAsset(config,ConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log("gzp 已自动创建EvolutionConfig");
            _cachedConfig = config;
            return _cachedConfig;
        }
    }
}