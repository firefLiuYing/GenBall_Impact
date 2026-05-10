using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GenBall.BattleSystem.Buff
{
    [CreateAssetMenu(fileName = "BuffModelConfig", menuName = "Buff/BuffModelConfig")]
    public class BuffModelConfig : ScriptableObject
    {
        [SerializeField] private List<BuffModel> buffModels=new();
        private readonly Dictionary<BuffId,BuffModel> _buffDict = new();
        public void Init()
        {
            _buffDict.Clear();
            foreach (var buffModel in buffModels)
            {
                _buffDict.TryAdd(buffModel.BuffId,buffModel);
            }
        }

        public BuffModel GetBuffModel(BuffId buffId)
        {
            return _buffDict.GetValueOrDefault(buffId);
        }
    }

    public static class ConfigProvider
    {
        private const string BuffModelConfigPath = "Assets/AssetBundles/Config/BuffModelConfig.asset";
        private static BuffModelConfig _cachedConfig;
        private static bool _configInitialized=false;
        #if UNITY_EDITOR
        public static BuffModelConfig GetOrCreateBuffModelConfig()
        {
            if(_cachedConfig!=null)  return _cachedConfig;
            var guids=AssetDatabase.FindAssets("t:BuffModelConfig");
            if (guids.Length > 1)
            {
                Debug.LogError("gzp 发现多个BuffModelConfig，请只保留一个");
                return null;
            }
            if (guids.Length == 1)
            {
                var path=AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedConfig= AssetDatabase.LoadAssetAtPath<BuffModelConfig>(path);
                if (!_configInitialized)
                {
                    _cachedConfig.Init();
                    _configInitialized=true;
                }
                return _cachedConfig;
            }
            var config=ScriptableObject.CreateInstance<BuffModelConfig>();
            AssetDatabase.CreateAsset(config,BuffModelConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log("gzp 已自动创建BuffModelConfig");
            _cachedConfig = config;
            if (!_configInitialized)
            {
                _cachedConfig.Init();
                _configInitialized=true;
            }
            return _cachedConfig;
        }
        #endif
    }
    [Serializable]
    public class BuffModel
    {
        [SerializeField,Tooltip("根据这个名字来查找对应buff效果")] private BuffId buffId;
        [SerializeField] private string displayName;
        [SerializeField] private bool canMultiExist;
        [SerializeField,Tooltip("触发优先级")] private int priority;
        [SerializeField] private List<string> tags;
        [SerializeField] private List<BuffParam> parameters;
        [SerializeField,Tooltip("不可叠层时填1")] private int maxStack;
        
        public BuffId BuffId => buffId;
        public string DisplayName => displayName;
        public bool CanMultiExist => canMultiExist;
        public int Priority => priority;
        public IReadOnlyList<string> Tags => tags;
        public IReadOnlyList<BuffParam> Parameters => parameters;
        public int MaxStack => maxStack;
    }

    [Serializable]
    public struct BuffParam
    {
        [SerializeField] private string key;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private string stringValue;
        
        public string Key => key;
        public int IntValue => intValue;
        public float FloatValue => floatValue;
        public string StringValue => stringValue;
    }
}