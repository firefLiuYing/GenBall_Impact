using System;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.Resource
{
    /// <summary>
    /// 资源管理器（新体系基建）
    /// C 前缀用于避免与旧体系 ResourceManager 命名冲突
    /// </summary>
    public class CResourceManager : Singleton<CResourceManager>
    {
        private IResourceHelper _helper;

        protected override void Init()
        {
            // 初始化逻辑（如果需要）
        }

        /// <summary>
        /// 设置资源加载助手（在 FrameworkDefault 中调用）
        /// </summary>
        public void SetHelper(IResourceHelper helper)
        {
            _helper = helper;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed)
        {
            if (_helper == null)
            {
                Debug.LogError("[CResourceManager] Helper is not set!");
                onLoadFailed?.Invoke("Helper is not set");
                return;
            }
            _helper.Load(path, onLoadSuccess, onLoadFailed);
        }

        /// <summary>
        /// 异步加载资源（带进度）
        /// </summary>
        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
        {
            if (_helper == null)
            {
                Debug.LogError("[CResourceManager] Helper is not set!");
                onLoadFailed?.Invoke("Helper is not set");
                return;
            }
            _helper.Load(path, onLoadSuccess, onLoadFailed, onProgress);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T LoadSync<T>(string path) where T : UnityEngine.Object
        {
            if (_helper == null)
            {
                Debug.LogError("[CResourceManager] Helper is not set!");
                return null;
            }
            return _helper.LoadSync<T>(path);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public void Unload(string path, bool unloadAllLoadedObjects = false)
        {
            if (_helper == null)
            {
                Debug.LogError("[CResourceManager] Helper is not set!");
                return;
            }
            _helper.Unload(path, unloadAllLoadedObjects);
        }
    }
}
