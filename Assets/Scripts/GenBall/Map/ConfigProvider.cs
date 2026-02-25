using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GenBall.Map
{
    public static class ConfigProvider
    {
        private const string SavePointConfigPath = "Assets/AssetBundles/Config/MapModel.asset";
        public static MapModel GetOrCreateMapConfig()
        {
            var guids=AssetDatabase.FindAssets("t:MapModel");
            if (guids.Length > 1)
            {
                Debug.LogError("发现多个SavePointConfig，请只保留一个");
                return null;
            }

            if (guids.Length == 1)
            {
                var path=AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<MapModel>(path);
            }
            
            var index=ScriptableObject.CreateInstance<MapModel>();
            AssetDatabase.CreateAsset(index,SavePointConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log("已自动创建SavePointConfig");
            return index;
        }
    }
}