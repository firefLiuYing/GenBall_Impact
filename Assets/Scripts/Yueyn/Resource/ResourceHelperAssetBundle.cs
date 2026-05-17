using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.Resource
{
    /// <summary>
    /// AssetBundle 资源加载助手
    /// 用于运行时加载 AB 包中的资源
    /// 完全复刻 ResourceSystemAssetBundle 的逻辑
    /// </summary>
    public class ResourceHelperAssetBundle : IResourceHelper
    {
        private AssetBundleLoader _loader;
        private Dictionary<string, string> _assetToBundleMap;
        private CoroutineRunner _coroutineRunner;
        private bool _isInitialized;

        public ResourceHelperAssetBundle()
        {
            Init();
        }

        private void Init()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[ResourceHelperAssetBundle] Already initialized");
                return;
            }

            // 创建协程运行器
            _coroutineRunner = CoroutineRunner.Instance;

            // 初始化AssetBundleLoader
            _loader = new AssetBundleLoader();
            string bundleRootPath = GetBundleRootPath();

            if (!_loader.Initialize(bundleRootPath))
            {
                Debug.LogError("[ResourceHelperAssetBundle] Failed to initialize AssetBundleLoader");
                return;
            }

            // 初始化资源映射表
            _assetToBundleMap = new Dictionary<string, string>();

            _isInitialized = true;
            Debug.Log("[ResourceHelperAssetBundle] Initialized");
        }

        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed)
        {
            if (!_isInitialized)
            {
                onLoadFailed?.Invoke("ResourceHelper not initialized");
                return;
            }

            _coroutineRunner.StartCoroutine(LoadAssetCoroutine(path, onLoadSuccess, onLoadFailed, null));
        }

        public void Load(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
        {
            if (!_isInitialized)
            {
                onLoadFailed?.Invoke("ResourceHelper not initialized");
                return;
            }

            _coroutineRunner.StartCoroutine(LoadAssetCoroutine(path, onLoadSuccess, onLoadFailed, onProgress));
        }

        public T LoadSync<T>(string path) where T : UnityEngine.Object
        {
            if (!_isInitialized)
            {
                Debug.LogError("[ResourceHelperAssetBundle] Not initialized");
                return null;
            }

            // 解析路径
            if (!ResolveAssetPath(path, out string bundleName, out string assetName))
            {
                Debug.LogError($"[ResourceHelperAssetBundle] Failed to resolve path: {path}");
                return null;
            }

            // 加载Bundle（含依赖）
            AssetBundle bundle = _loader.LoadBundle(bundleName, loadDependencies: true);
            if (bundle == null)
            {
                Debug.LogError($"[ResourceHelperAssetBundle] Failed to load bundle: {bundleName}");
                return null;
            }

            // 从Bundle中加载资源
            T asset = _loader.LoadAsset<T>(bundleName, assetName);
            if (asset == null)
            {
                Debug.LogError($"[ResourceHelperAssetBundle] Failed to load asset: {assetName} from bundle: {bundleName}");
            }

            return asset;
        }

        public void Unload(string path, bool unloadAllLoadedObjects = false)
        {
            if (!_isInitialized) return;

            if (ResolveAssetPath(path, out string bundleName, out _))
            {
                _loader.UnloadBundle(bundleName, unloadAllLoadedObjects);
            }
        }

        private IEnumerator LoadAssetCoroutine(string path, Action<object> onLoadSuccess, Action<string> onLoadFailed, Action<float> onProgress)
        {
            onProgress?.Invoke(0f);

            // 1. 解析路径
            if (!ResolveAssetPath(path, out string bundleName, out string assetName))
            {
                onLoadFailed?.Invoke($"Failed to resolve path: {path}");
                yield break;
            }

            onProgress?.Invoke(0.2f);

            // 2. 异步加载Bundle（含依赖）
            // bool bundleLoaded = false;
            AssetBundle loadedBundle = null;

            yield return _loader.LoadBundleAsync(bundleName, (bundle) =>
            {
                loadedBundle = bundle;
                // bundleLoaded = true;
            });

            if (loadedBundle == null)
            {
                onLoadFailed?.Invoke($"Failed to load bundle: {bundleName}");
                yield break;
            }

            onProgress?.Invoke(0.6f);

            // 3. 异步加载资源
            UnityEngine.Object asset = null;

            yield return _loader.LoadAssetAsync<UnityEngine.Object>(bundleName, assetName, (loadedAsset) =>
            {
                asset = loadedAsset;
            });

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
                Debug.Log($"[ResourceHelperAssetBundle] Found cached mapping: {path} → bundle: {bundleName}, asset: {assetName}");
                return true;
            }

            // 否则根据路径推断Bundle名
            bundleName = InferBundleNameFromPath(path);

            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogError($"[ResourceHelperAssetBundle] Cannot infer bundle name from path: {path}");
                return false;
            }

            Debug.Log($"[ResourceHelperAssetBundle] Inferred mapping: {path} → bundle: {bundleName}, asset: {assetName}");

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
}
