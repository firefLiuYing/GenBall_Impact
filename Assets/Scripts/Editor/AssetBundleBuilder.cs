using System.IO;
using UnityEditor;
using UnityEngine;

namespace Yueyn.Editor
{
    /// <summary>
    /// AssetBundle构建工具
    /// 提供菜单项来构建AssetBundle
    /// </summary>
    public class AssetBundleBuilder
    {
        // 构建输出路径
        private static string OutputPath => Path.Combine(Application.streamingAssetsPath, "AssetBundles");

        [MenuItem("Tools/AssetBundle/Build AssetBundles (Current Platform)")]
        public static void BuildAssetBundles()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildAssetBundlesForPlatform(buildTarget);
        }

        [MenuItem("Tools/AssetBundle/Build AssetBundles (Windows)")]
        public static void BuildAssetBundlesWindows()
        {
            BuildAssetBundlesForPlatform(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Tools/AssetBundle/Build AssetBundles (Android)")]
        public static void BuildAssetBundlesAndroid()
        {
            BuildAssetBundlesForPlatform(BuildTarget.Android);
        }

        [MenuItem("Tools/AssetBundle/Build AssetBundles (iOS)")]
        public static void BuildAssetBundlesiOS()
        {
            BuildAssetBundlesForPlatform(BuildTarget.iOS);
        }

        [MenuItem("Tools/AssetBundle/Clear AssetBundles")]
        public static void ClearAssetBundles()
        {
            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
                Debug.Log($"[AssetBundleBuilder] Cleared AssetBundles at: {OutputPath}");
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/AssetBundle/Open Output Folder")]
        public static void OpenOutputFolder()
        {
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            EditorUtility.RevealInFinder(OutputPath);
        }

        /// <summary>
        /// 为指定平台构建AssetBundle
        /// </summary>
        private static void BuildAssetBundlesForPlatform(BuildTarget buildTarget)
        {
            // 获取平台名称
            string platformName = GetPlatformName(buildTarget);
            string platformOutputPath = Path.Combine(OutputPath, platformName);

            // 创建输出目录
            if (!Directory.Exists(platformOutputPath))
            {
                Directory.CreateDirectory(platformOutputPath);
            }

            Debug.Log($"[AssetBundleBuilder] Building AssetBundles for {platformName}...");
            Debug.Log($"[AssetBundleBuilder] Output path: {platformOutputPath}");

            // 构建AssetBundle
            BuildPipeline.BuildAssetBundles(
                platformOutputPath,
                BuildAssetBundleOptions.None,
                buildTarget
            );

            Debug.Log($"[AssetBundleBuilder] Build completed for {platformName}");

            // 刷新AssetDatabase
            AssetDatabase.Refresh();

            // 显示构建结果
            ShowBuildResult(platformOutputPath);
        }

        /// <summary>
        /// 获取平台名称
        /// </summary>
        private static string GetPlatformName(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "StandaloneWindows64";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.StandaloneOSX:
                    return "StandaloneOSX";
                case BuildTarget.StandaloneLinux64:
                    return "StandaloneLinux64";
                default:
                    return buildTarget.ToString();
            }
        }

        /// <summary>
        /// 显示构建结果
        /// </summary>
        private static void ShowBuildResult(string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Debug.LogError($"[AssetBundleBuilder] Output path not found: {outputPath}");
                return;
            }

            // 统计构建的Bundle数量
            string[] bundles = Directory.GetFiles(outputPath, "*", SearchOption.TopDirectoryOnly);
            int bundleCount = 0;
            long totalSize = 0;

            foreach (string bundle in bundles)
            {
                // 排除.manifest文件和主manifest
                if (!bundle.EndsWith(".manifest") && !bundle.EndsWith(".meta"))
                {
                    FileInfo fileInfo = new FileInfo(bundle);
                    bundleCount++;
                    totalSize += fileInfo.Length;
                }
            }

            Debug.Log($"[AssetBundleBuilder] Built {bundleCount} bundles, Total size: {FormatBytes(totalSize)}");
        }

        /// <summary>
        /// 格式化字节大小
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
