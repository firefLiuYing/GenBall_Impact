using UnityEditor;
using UnityEngine;

namespace Yueyn.Editor
{
    /// <summary>
    /// AssetBundle调试工具
    /// 用于查看Bundle中的资源列表
    /// </summary>
    public class AssetBundleDebugger
    {
        [MenuItem("Tools/AssetBundle/Debug - Show Bundle Contents")]
        public static void ShowBundleContents()
        {
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();

            if (bundleNames.Length == 0)
            {
                Debug.Log("[AssetBundleDebugger] No bundles found. Run 'Auto Set Bundle Names' first.");
                return;
            }

            Debug.Log($"[AssetBundleDebugger] Found {bundleNames.Length} bundles:\n");

            foreach (string bundleName in bundleNames)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                Debug.Log($"Bundle: {bundleName} ({assetPaths.Length} assets)");

                foreach (string assetPath in assetPaths)
                {
                    Debug.Log($"  - {assetPath}");
                }

                Debug.Log("");
            }
        }

        [MenuItem("Tools/AssetBundle/Debug - Test Path Resolution")]
        public static void TestPathResolution()
        {
            string[] testPaths = new string[]
            {
                "Assets/AssetBundles/UI/MainHud/Form/TestForm.prefab",
                "Assets/Resources/UI/TestForm.prefab",
                "UI/TestForm.prefab",
                "Assets/AssetBundles/Common/Player/Prefab/Player.prefab"
            };

            Debug.Log("[AssetBundleDebugger] Testing path resolution:\n");

            foreach (string path in testPaths)
            {
                string bundleName = InferBundleNameFromPath(path);
                Debug.Log($"Path: {path}");
                Debug.Log($"  → Bundle: {bundleName}\n");
            }
        }

        private static string InferBundleNameFromPath(string path)
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
    }
}
