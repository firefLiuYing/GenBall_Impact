using System;
using Yueyn.Main;

namespace Yueyn.Resource
{
    public interface IResourceSystem : ISystem
    {
        /// <summary>
        /// 异步加载资源
        /// </summary>
        void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed);

        /// <summary>
        /// 异步加载资源（带进度）
        /// </summary>
        void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        T LoadSync<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        /// 卸载资源
        /// </summary>
        void Unload(string path, bool unloadAllLoadedObjects = false);
    }
}