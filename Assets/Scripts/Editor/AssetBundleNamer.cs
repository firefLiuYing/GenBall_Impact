using System.IO;
using UnityEditor;
using UnityEngine;

namespace Yueyn.Editor
{
    /// <summary>
    /// AssetBundle名称设置工具
    /// 自动为Assets/AssetBundles目录下的资源设置Bundle名称
    /// </summary>
    public class AssetBundleNamer
    {
        [MenuItem("Tools/AssetBundle/Auto Set Bundle Names")]
        public static void AutoSetBundleNames()
        {
            string assetBundlesPath = "Assets/AssetBundles";

            if (!Directory.Exists(assetBundlesPath))
            {
                Debug.LogError($"[AssetBundleNamer] AssetBundles directory not found: {assetBundlesPath}");
                return;
            }

            int count = 0;

            // 获取所有子目录（每个子目录对应一个Bundle）
            string[] directories = Directory.GetDirectories(assetBundlesPath);

            foreach (string dir in directories)
            {
                // 获取目录名作为Bundle名（转小写）
                string bundleName = Path.GetFileName(dir).ToLower();

                // 获取该目录下的所有资源文件
                string[] assetPaths = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

                foreach (string assetPath in assetPaths)
                {
                    // 跳过.meta文件
                    if (assetPath.EndsWith(".meta"))
                        continue;

                    // 转换为Unity路径格式
                    string unityPath = assetPath.Replace("\\", "/");

                    // 获取AssetImporter
                    AssetImporter importer = AssetImporter.GetAtPath(unityPath);
                    if (importer != null)
                    {
                        // 设置Bundle名称
                        if (importer.assetBundleName != bundleName)
                        {
                            importer.assetBundleName = bundleName;
                            count++;
                        }
                    }
                }
            }

            // 刷新AssetDatabase
            AssetDatabase.Refresh();

            Debug.Log($"[AssetBundleNamer] Set bundle names for {count} assets");

            // 显示所有Bundle名称
            ShowAllBundleNames();
        }

        [MenuItem("Tools/AssetBundle/Clear All Bundle Names")]
        public static void ClearAllBundleNames()
        {
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            int count = 0;

            foreach (string bundleName in bundleNames)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                foreach (string assetPath in assetPaths)
                {
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                    if (importer != null)
                    {
                        importer.assetBundleName = "";
                        count++;
                    }
                }
            }

            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();

            Debug.Log($"[AssetBundleNamer] Cleared bundle names for {count} assets");
        }

        [MenuItem("Tools/AssetBundle/Show All Bundle Names")]
        public static void ShowAllBundleNames()
        {
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();

            if (bundleNames.Length == 0)
            {
                Debug.Log("[AssetBundleNamer] No bundle names found");
                return;
            }

            Debug.Log($"[AssetBundleNamer] Found {bundleNames.Length} bundles:");

            foreach (string bundleName in bundleNames)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                Debug.Log($"  - {bundleName} ({assetPaths.Length} assets)");
            }
        }
    }
}
