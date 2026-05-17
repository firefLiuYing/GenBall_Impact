using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Yueyn.Resource
{
    /// <summary>
    /// 编辑器资源加载助手
    /// 使用 AssetDatabase.LoadAssetAtPath 直接加载资源（仅编辑器可用）
    /// </summary>
    public class ResourceHelperEditor : IResourceHelper
    {
        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed)
        {
            #if UNITY_EDITOR
            try
            {
                var resource = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (resource != null)
                {
                    onLoadSuccess?.Invoke(resource);
                }
                else
                {
                    Debug.LogWarning($"[ResourceHelperEditor] Resource not found: {path}");
                    onLoadFailed?.Invoke($"Resource not found: {path}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceHelperEditor] Failed to load: {path}, Error: {e.Message}");
                onLoadFailed?.Invoke($"Failed to load resource: {path}, Error: {e.Message}");
            }
            #else
            Debug.LogError("[ResourceHelperEditor] AssetDatabase is only available in Editor!");
            onLoadFailed?.Invoke("AssetDatabase is only available in Editor!");
            #endif
        }

        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
        {
            onProgress?.Invoke(0f);
            Load(path, onLoadSuccess, onLoadFailed);
            onProgress?.Invoke(1f);
        }

        public T LoadSync<T>(string path) where T : UnityEngine.Object
        {
            #if UNITY_EDITOR
            try
            {
                T resource = AssetDatabase.LoadAssetAtPath<T>(path);
                if (resource == null)
                {
                    Debug.LogWarning($"[ResourceHelperEditor] Resource not found: {path}");
                }
                return resource;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceHelperEditor] Failed to load: {path}, Error: {e.Message}");
                return null;
            }
            #else
            Debug.LogError("[ResourceHelperEditor] AssetDatabase is only available in Editor!");
            return null;
            #endif
        }

        public void Unload(string path, bool unloadAllLoadedObjects = false)
        {
            // AssetDatabase 加载的资源由 Unity 自动管理，不需要手动卸载
        }
    }
}
