using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GenBall.Map
{
    public static class SceneMapIndexProvider
    {
        private const string DefaultPath = "Assets/AssetBundles/Config/SceneMapIndex.asset";

        public static MapConfig GetMapConfig(string sceneName)
        {
            var index = GetOrCreateSceneMapIndex();
            return index.mapConfigChooses.FirstOrDefault(c=>c.mapConfig.sceneName==sceneName)?.mapConfig;
        }
        private static SceneMapIndex GetOrCreateSceneMapIndex()
        {
            var guids=AssetDatabase.FindAssets("t:SceneMapIndex");
            if (guids.Length > 1)
            {
                Debug.LogError("发现多个SceneIndex，请只保留一个");
                return null;
            }

            if (guids.Length == 1)
            {
                var path=AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<SceneMapIndex>(path);
            }
            
            var index=ScriptableObject.CreateInstance<SceneMapIndex>();
            AssetDatabase.CreateAsset(index,DefaultPath);
            AssetDatabase.SaveAssets();
            Debug.Log("已自动创建SceneIndex");
            return index;
        }
        
        
        public static void RegisterMapConfig(MapConfig mapConfig)
        {
            var index = SceneMapIndexProvider.GetOrCreateSceneMapIndex();
            if(index==null)return;
            index.mapConfigChooses.RemoveAll(m=>m.mapConfig.sceneName==mapConfig.sceneName);
            index.mapConfigChooses.Add(new MapConfigChoose(){mapConfig = mapConfig,selected = true});
            EditorUtility.SetDirty(index);
            AssetDatabase.SaveAssets();
        }
    }
}