using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yueyn.Resource
{
    /// <summary>
    /// 基于AssetBundle的资源系统
    /// 用于运行时加载AB包中的资源
    /// </summary>
    public class ResourceSystemAssetBundle : IResourceSystem
    {
        private AssetBundleLoader _loader;
        private Dictionary<string, string> _assetToBundleMap;
        private MonoBehaviour _coroutineRunner;
        private bool _isInitialized;

        public void Init()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[ResourceSystemAssetBundle] Already initialized");
                return;
            }

            // 创建协程运行器
            GameObject go = new GameObject("ResourceSystemCoroutineRunner");
            GameObject.DontDestroyOnLoad(go);
            _coroutineRunner = go.AddComponent<CoroutineRunner>();

            // 初始化AssetBundleLoader
            _loader = new AssetBundleLoader();
            string bundleRootPath = GetBundleRootPath();

            if (!_loader.Initialize(bundleRootPath))
            {
                Debug.LogError("[ResourceSystemAssetBundle] Failed to initialize AssetBundleLoader");
                return;
            }

            // 初始化资源映射表
            _assetToBundleMap = new Dictionary<string, string>();

            _isInitialized = true;
            Debug.Log("[ResourceSystemAssetBundle] Initialized");
        }

        public void UnInit()
        {
            if (!_isInitialized) return;

            _loader?.UnloadAll(true);
            _assetToBundleMap?.Clear();

            if (_coroutineRunner != null)
            {
                GameObject.Destroy(_coroutineRunner.gameObject);
                _coroutineRunner = null;
            }

            _isInitialized = false;
            Debug.Log("[ResourceSystemAssetBundle] UnInitialized");
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed)
        {
            if (!_isInitialized)
            {
                onLoadFailed?.Invoke("ResourceSystem not initialized");
                return;
            }

            _coroutineRunner.StartCoroutine(LoadAssetCoroutine(path, onLoadSuccess, onLoadFailed, null));
        }

        /// <summary>
        /// 异步加载资源（带进度）
        /// </summary>
        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
        {
            if (!_isInitialized)
            {
                onLoadFailed?.Invoke("ResourceSystem not initialized");
                return;
            }

            _coroutineRunner.StartCoroutine(LoadAssetCoroutine(path, onLoadSuccess, onLoadFailed, onProgress));
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T LoadSync<T>(string path) where T : UnityEngine.Object
        {
            if (!_isInitialized)
            {
                Debug.LogError("[ResourceSystemAssetBundle] ResourceSystem not initialized");
                return null;
            }

            // 解析路径获取Bundle名和Asset名
            if (!ResolveAssetPath(path, out string bundleName, out string assetName))
            {
                Debug.LogError($"[ResourceSystemAssetBundle] Failed to resolve path: {path}");
                return null;
            }

            // 加载Bundle
            AssetBundle bundle = _loader.LoadBundle(bundleName);
            if (bundle == null)
            {
                Debug.LogError($"[ResourceSystemAssetBundle] Failed to load bundle: {bundleName}");
                return null;
            }

            // 加载Asset
            T asset = _loader.LoadAsset<T>(bundleName, assetName);
            return asset;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public void Unload(string path, bool unloadAllLoadedObjects = false)
        {
            if (!_isInitialized) return;

            if (!ResolveAssetPath(path, out string bundleName, out string assetName))
            {
                Debug.LogWarning($"[ResourceSystemAssetBundle] Failed to resolve path for unload: {path}");
                return;
            }

            _loader.UnloadBundle(bundleName, unloadAllLoadedObjects);
        }

        /// <summary>
        /// 异步加载资源的协程
        /// </summary>
        private IEnumerator LoadAssetCoroutine(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
        {
            onProgress?.Invoke(0f);

            // 解析路径
            if (!ResolveAssetPath(path, out string bundleName, out string assetName))
            {
                onLoadFailed?.Invoke($"Failed to resolve path: {path}");
                yield break;
            }

            onProgress?.Invoke(0.3f);

            // 加载Bundle
            AssetBundle bundle = null;
            yield return _loader.LoadBundleAsync(bundleName, (b) => { bundle = b; });

            if (bundle == null)
            {
                onLoadFailed?.Invoke($"Failed to load bundle: {bundleName}");
                yield break;
            }

            onProgress?.Invoke(0.7f);

            // 加载Asset
            UnityEngine.Object asset = null;
            yield return _loader.LoadAssetAsync<UnityEngine.Object>(bundleName, assetName, (a) => { asset = a; });

            if (asset == null)
            {
                onLoadFailed?.Invoke($"Failed to load asset: {assetName} from bundle: {bundleName}");
                yield break;
            }

            onProgress?.Invoke(1f);
            onLoadSuccess?.Invoke(asset);
        }

        /// <summary>
        /// 解析资源路径，获取Bundle名和Asset名
        /// </summary>
        private bool ResolveAssetPath(string path, out string bundleName, out string assetName)
        {
            bundleName = null;
            assetName = path;

            // 如果已经有映射，直接返回
            if (_assetToBundleMap.TryGetValue(path, out bundleName))
            {
                Debug.Log($"[ResourceSystemAssetBundle] Found cached mapping: {path} → bundle: {bundleName}, asset: {assetName}");
                return true;
            }

            // 否则根据路径推断Bundle名
            bundleName = InferBundleNameFromPath(path);

            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogError($"[ResourceSystemAssetBundle] Cannot infer bundle name from path: {path}");
                return false;
            }

            Debug.Log($"[ResourceSystemAssetBundle] Inferred mapping: {path} → bundle: {bundleName}, asset: {assetName}");

            // 缓存映射
            _assetToBundleMap[path] = bundleName;
            return true;
        }

        /// <summary>
        /// 从路径推断Bundle名
        /// </summary>
        private string InferBundleNameFromPath(string path)
        {
            // 移除 "Assets/" 前缀
            if (path.StartsWith("Assets/"))
            {
                path = path.Substring(7);
            }

            // 移除 "Resources/" 前缀（如果有）
            if (path.StartsWith("Resources/"))
            {
                path = path.Substring(10);
            }

            // 移除 "AssetBundles/" 前缀（如果有）
            if (path.StartsWith("AssetBundles/"))
            {
                path = path.Substring(13);
            }

            // 获取第一级目录名作为Bundle名
            int slashIndex = path.IndexOf('/');
            if (slashIndex > 0)
            {
                string bundleName = path.Substring(0, slashIndex).ToLower();
                return bundleName;
            }

            return null;
        }

        /// <summary>
        /// 获取AB包根路径
        /// </summary>
        private string GetBundleRootPath()
        {
            string platform = GetPlatformName();
            return System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles", platform);
        }

        /// <summary>
        /// 获取平台名称
        /// </summary>
        private string GetPlatformName()
        {
#if UNITY_EDITOR
            return "StandaloneWindows64";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            return "StandaloneWindows64";
#endif
        }
    }

    /// <summary>
    /// 协程运行器（用于在非MonoBehaviour类中运行协程）
    /// </summary>
    internal class CoroutineRunner : MonoBehaviour
    {
    }
}