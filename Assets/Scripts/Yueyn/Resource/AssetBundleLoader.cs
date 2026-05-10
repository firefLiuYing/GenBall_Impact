using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yueyn.Resource
{
    /// <summary>
    /// AssetBundle加载器
    /// 负责AB包的加载、卸载、依赖管理和引用计数
    /// </summary>
    public class AssetBundleLoader
    {
        // AB包缓存
        private Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();

        // 引用计数
        private Dictionary<string, int> _referenceCount = new Dictionary<string, int>();

        // 依赖关系缓存
        private Dictionary<string, string[]> _dependencies = new Dictionary<string, string[]>();

        // 主Manifest
        private AssetBundleManifest _manifest;

        // AB包根路径
        private string _bundleRootPath;

        /// <summary>
        /// 初始化
        /// </summary>
        public bool Initialize(string bundleRootPath)
        {
            _bundleRootPath = bundleRootPath;

            // 获取平台名称（主manifest文件名）
            string platformName = System.IO.Path.GetFileName(_bundleRootPath);
            string manifestPath = System.IO.Path.Combine(_bundleRootPath, platformName);

            if (!System.IO.File.Exists(manifestPath))
            {
                Debug.LogError($"[AssetBundleLoader] Manifest not found at: {manifestPath}");
                Debug.LogError($"[AssetBundleLoader] Bundle root path: {_bundleRootPath}");
                return false;
            }

            AssetBundle manifestBundle = AssetBundle.LoadFromFile(manifestPath);
            if (manifestBundle == null)
            {
                Debug.LogError($"[AssetBundleLoader] Failed to load manifest bundle from: {manifestPath}");
                return false;
            }

            _manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            manifestBundle.Unload(false);

            if (_manifest == null)
            {
                Debug.LogError($"[AssetBundleLoader] Failed to load AssetBundleManifest");
                return false;
            }

            Debug.Log($"[AssetBundleLoader] Initialized with root path: {_bundleRootPath}");
            Debug.Log($"[AssetBundleLoader] Manifest file: {manifestPath}");
            return true;
        }

        /// <summary>
        /// 同步加载AB包（含依赖）
        /// </summary>
        public AssetBundle LoadBundle(string bundleName, bool loadDependencies = true)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogError("[AssetBundleLoader] Bundle name is null or empty");
                return null;
            }

            // 如果已加载，增加引用计数
            if (_loadedBundles.ContainsKey(bundleName))
            {
                _referenceCount[bundleName]++;
                Debug.Log($"[AssetBundleLoader] Bundle already loaded: {bundleName}, RefCount: {_referenceCount[bundleName]}");
                return _loadedBundles[bundleName];
            }

            // 加载依赖
            if (loadDependencies)
            {
                string[] dependencies = GetDependencies(bundleName);
                foreach (string dep in dependencies)
                {
                    LoadBundle(dep, true);
                }
            }

            // 加载Bundle
            string bundlePath = System.IO.Path.Combine(_bundleRootPath, bundleName);
            if (!System.IO.File.Exists(bundlePath))
            {
                Debug.LogError($"[AssetBundleLoader] Bundle file not found: {bundlePath}");
                return null;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Debug.LogError($"[AssetBundleLoader] Failed to load bundle: {bundleName}");
                return null;
            }

            _loadedBundles[bundleName] = bundle;
            _referenceCount[bundleName] = 1;

            Debug.Log($"[AssetBundleLoader] Loaded bundle: {bundleName}");
            return bundle;
        }

        /// <summary>
        /// 异步加载AB包（含依赖）
        /// </summary>
        public IEnumerator LoadBundleAsync(string bundleName, Action<AssetBundle> onComplete, bool loadDependencies = true)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogError("[AssetBundleLoader] Bundle name is null or empty");
                onComplete?.Invoke(null);
                yield break;
            }

            // 如果已加载，增加引用计数
            if (_loadedBundles.ContainsKey(bundleName))
            {
                _referenceCount[bundleName]++;
                Debug.Log($"[AssetBundleLoader] Bundle already loaded: {bundleName}, RefCount: {_referenceCount[bundleName]}");
                onComplete?.Invoke(_loadedBundles[bundleName]);
                yield break;
            }

            // 加载依赖
            if (loadDependencies)
            {
                string[] dependencies = GetDependencies(bundleName);
                foreach (string dep in dependencies)
                {
                    bool depLoaded = false;
                    yield return LoadBundleAsync(dep, (bundle) => { depLoaded = true; }, true);
                    while (!depLoaded) yield return null;
                }
            }

            // 加载Bundle
            string bundlePath = System.IO.Path.Combine(_bundleRootPath, bundleName);
            if (!System.IO.File.Exists(bundlePath))
            {
                Debug.LogError($"[AssetBundleLoader] Bundle file not found: {bundlePath}");
                onComplete?.Invoke(null);
                yield break;
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return request;

            if (request.assetBundle == null)
            {
                Debug.LogError($"[AssetBundleLoader] Failed to load bundle: {bundleName}");
                onComplete?.Invoke(null);
                yield break;
            }

            _loadedBundles[bundleName] = request.assetBundle;
            _referenceCount[bundleName] = 1;

            Debug.Log($"[AssetBundleLoader] Loaded bundle async: {bundleName}");
            onComplete?.Invoke(request.assetBundle);
        }

        /// <summary>
        /// 卸载AB包（引用计数）
        /// </summary>
        public void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            if (string.IsNullOrEmpty(bundleName) || !_loadedBundles.ContainsKey(bundleName))
            {
                return;
            }

            // 减少引用计数
            _referenceCount[bundleName]--;

            if (_referenceCount[bundleName] <= 0)
            {
                // 卸载依赖
                string[] dependencies = GetDependencies(bundleName);
                foreach (string dep in dependencies)
                {
                    UnloadBundle(dep, unloadAllLoadedObjects);
                }

                // 卸载Bundle
                _loadedBundles[bundleName].Unload(unloadAllLoadedObjects);
                _loadedBundles.Remove(bundleName);
                _referenceCount.Remove(bundleName);

                Debug.Log($"[AssetBundleLoader] Unloaded bundle: {bundleName}");
            }
            else
            {
                Debug.Log($"[AssetBundleLoader] Decreased ref count for {bundleName}, RefCount: {_referenceCount[bundleName]}");
            }
        }

        /// <summary>
        /// 从已加载的Bundle中同步加载资源
        /// </summary>
        public T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            if (!_loadedBundles.ContainsKey(bundleName))
            {
                Debug.LogError($"[AssetBundleLoader] Bundle not loaded: {bundleName}");
                return null;
            }

            Debug.Log($"[AssetBundleLoader] Loading asset: {assetName} from bundle: {bundleName}");

            // 先尝试直接加载
            T asset = _loadedBundles[bundleName].LoadAsset<T>(assetName);

            if (asset == null)
            {
                // 如果失败，列出bundle中所有资源
                string[] allAssets = _loadedBundles[bundleName].GetAllAssetNames();
                Debug.LogError($"[AssetBundleLoader] Asset not found: {assetName} in bundle {bundleName}");
                Debug.LogError($"[AssetBundleLoader] Available assets in bundle ({allAssets.Length}):");
                foreach (string availableAsset in allAssets)
                {
                    Debug.LogError($"  - {availableAsset}");
                }
            }

            return asset;
        }

        /// <summary>
        /// 从已加载的Bundle中异步加载资源
        /// </summary>
        public IEnumerator LoadAssetAsync<T>(string bundleName, string assetName, Action<T> onComplete) where T : UnityEngine.Object
        {
            if (!_loadedBundles.ContainsKey(bundleName))
            {
                Debug.LogError($"[AssetBundleLoader] Bundle not loaded: {bundleName}");
                onComplete?.Invoke(null);
                yield break;
            }

            AssetBundleRequest request = _loadedBundles[bundleName].LoadAssetAsync<T>(assetName);
            yield return request;

            if (request.asset == null)
            {
                Debug.LogError($"[AssetBundleLoader] Asset not found: {assetName} in bundle {bundleName}");
                onComplete?.Invoke(null);
            }
            else
            {
                onComplete?.Invoke(request.asset as T);
            }
        }

        /// <summary>
        /// 获取Bundle的依赖
        /// </summary>
        private string[] GetDependencies(string bundleName)
        {
            if (_dependencies.ContainsKey(bundleName))
            {
                return _dependencies[bundleName];
            }

            if (_manifest == null)
            {
                return new string[0];
            }

            string[] deps = _manifest.GetAllDependencies(bundleName);
            _dependencies[bundleName] = deps;
            return deps;
        }

        /// <summary>
        /// 清理所有已加载的Bundle
        /// </summary>
        public void UnloadAll(bool unloadAllLoadedObjects = false)
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                bundle.Unload(unloadAllLoadedObjects);
            }

            _loadedBundles.Clear();
            _referenceCount.Clear();
            _dependencies.Clear();

            Debug.Log("[AssetBundleLoader] Unloaded all bundles");
        }
    }
}
