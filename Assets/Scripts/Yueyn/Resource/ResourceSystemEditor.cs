using System;
using UnityEditor;
using UnityEngine;
using Yueyn.Main;

namespace Yueyn.Resource
{
    /// <summary>
    /// 资源系统默认实现（编辑器模式）
    /// 使用Unity的Resources.Load进行加载
    /// </summary>
    public class ResourceSystemEditor : IResourceSystem
    {
        public void Init()
        {
            Debug.Log("[ResourceSystemDefault] Initialized (Editor Mode)");
        }

        public void UnInit()
        {
            Debug.Log("[ResourceSystemDefault] UnInitialized");
        }

        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed)
        {
            try
            {
                // 移除扩展名（Resources.Load不需要扩展名）
                string resourcePath = RemoveExtension(path);

                var resource = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (resource != null)
                {
                    onLoadSuccess?.Invoke(resource);
                }
                else
                {
                    Debug.LogWarning($"[ResourceSystemDefault] Resource not found: {resourcePath}");
                    onLoadFailed?.Invoke($"Resource not found: {resourcePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceSystemDefault] Failed to load: {path}, Error: {e.Message}");
                onLoadFailed?.Invoke($"Failed to load resource: {path}, Error: {e.Message}");
            }
        }

        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
        {
            onProgress?.Invoke(0f);
            Load(path, onLoadSuccess, onLoadFailed);
            onProgress?.Invoke(1f);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T LoadSync<T>(string path) where T : UnityEngine.Object
        {
            try
            {
                // 移除扩展名
                string resourcePath = RemoveExtension(path);

                T resource = AssetDatabase.LoadAssetAtPath<T>(path);
                if (resource == null)
                {
                    Debug.LogWarning($"[ResourceSystemDefault] Resource not found: {resourcePath}");
                }
                return resource;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceSystemDefault] Failed to load: {path}, Error: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 卸载资源（Resources不需要手动卸载，这里是空实现）
        /// </summary>
        public void Unload(string path, bool unloadAllLoadedObjects = false)
        {
            // Resources.Load加载的资源由Unity自动管理，不需要手动卸载
        }

        /// <summary>
        /// 移除路径中的扩展名和Assets/Resources/前缀
        /// </summary>
        private string RemoveExtension(string path)
        {
            // 移除 "Assets/" 前缀
            if (path.StartsWith("Assets/"))
            {
                path = path.Substring(7);
            }

            // 移除 "Resources/" 前缀
            if (path.StartsWith("Resources/"))
            {
                path = path.Substring(10);
            }

            // 移除扩展名
            int dotIndex = path.LastIndexOf('.');
            if (dotIndex > 0)
            {
                path = path.Substring(0, dotIndex);
            }

            return path;
        }
    }
}
