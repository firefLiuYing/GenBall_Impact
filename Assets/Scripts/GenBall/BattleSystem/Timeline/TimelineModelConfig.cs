using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GenBall.BattleSystem.Timeline
{
    [CreateAssetMenu(fileName = "TimelineModelConfig", menuName = "TimelineModelConfig")]
    public class TimelineModelConfig : ScriptableObject
    {
        public List<TimelineModel>  models = new();
        private readonly Dictionary<string, TimelineModel> _timelineModelMap = new();
        public TimelineModel GetModel(string modelName)=>_timelineModelMap.GetValueOrDefault(modelName);

        public void Init()
        {
            _timelineModelMap.Clear();
            foreach (var model in models)
            {
                _timelineModelMap.TryAdd(model.timelineId, model);
            }
        }
    }

    public static class ConfigProvider
    {
        private static bool _configInitialized = false;
        private static TimelineModelConfig _cachedConfig;
        private const string ConfigPath = "Assets/AssetBundles/Config/TimelineModelConfig.asset";

        #if UNITY_EDITOR
        public static TimelineModelConfig GetOrCreateConfig()
        {
            if(_cachedConfig!=null)  return _cachedConfig;
            var guids=AssetDatabase.FindAssets("t:TimelineModelConfig");
            if (guids.Length > 1)
            {
                Debug.LogError("gzp 发现多个TimelineModelConfig，请只保留一个");
                return null;
            }
            if (guids.Length == 1)
            {
                var path=AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedConfig= AssetDatabase.LoadAssetAtPath<TimelineModelConfig>(path);
                if (!_configInitialized)
                {
                    _cachedConfig.Init();
                    _configInitialized=true;
                }
                return _cachedConfig;
            }
            var config=ScriptableObject.CreateInstance<TimelineModelConfig>();
            AssetDatabase.CreateAsset(config,ConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log("gzp 已自动创建TimelineModelConfig");
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
    public class TimelineModel
    {
        public string timelineId;
        public List<TimelineSegment>  segments;
    }

    [Serializable]
    public class TimelineSegment
    {
        [Tooltip("结束的时间点")]public float endTime;
        public TimelineSegmentId segmentId;
        public List<TimelineParam> parameters;
    }
    [Serializable]
    public struct TimelineParam
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