using UnityEngine;
using Yueyn.Resource;

namespace GenBall.Test
{
    /// <summary>
    /// 资源管理器测试脚本
    /// 测试 CResourceManager 的同步/异步加载功能
    /// </summary>
    public class TestResourceManager : MonoBehaviour
    {
        [Header("测试配置")]
        [Tooltip("要加载的预制体路径（相对于 Assets）")]
        public string testPrefabPath = "Assets/Prefabs/TestCube.prefab";
        
        [Tooltip("是否测试同步加载")]
        public bool testSyncLoad = true;
        
        [Tooltip("是否测试异步加载")]
        public bool testAsyncLoad = true;

        private void Start()
        {
            Debug.Log("=== 开始测试 CResourceManager ===");
            
            if (testSyncLoad)
            {
                TestSynchronousLoad();
            }
            
            if (testAsyncLoad)
            {
                TestAsynchronousLoad();
            }
        }

        /// <summary>
        /// 测试同步加载
        /// </summary>
        private void TestSynchronousLoad()
        {
            Debug.Log("[TestResourceManager] 测试同步加载...");
            
            GameObject prefab = CResourceManager.Instance.LoadSync<GameObject>(testPrefabPath);
            
            if (prefab != null)
            {
                Debug.Log($"[TestResourceManager] ✅ 同步加载成功: {prefab.name}");
                
                // 实例化测试
                GameObject instance = Instantiate(prefab);
                instance.name = "SyncLoadedInstance";
                instance.transform.position = new Vector3(-2, 0, 0);
                
                Debug.Log($"[TestResourceManager] ✅ 实例化成功: {instance.name}");
            }
            else
            {
                Debug.LogError($"[TestResourceManager] ❌ 同步加载失败: {testPrefabPath}");
            }
        }

        /// <summary>
        /// 测试异步加载
        /// </summary>
        private void TestAsynchronousLoad()
        {
            Debug.Log("[TestResourceManager] 测试异步加载...");
            
            CResourceManager.Instance.Load(
                testPrefabPath,
                onLoadSuccess: (obj) =>
                {
                    GameObject prefab = obj as GameObject;
                    if (prefab != null)
                    {
                        Debug.Log($"[TestResourceManager] ✅ 异步加载成功: {prefab.name}");
                        
                        // 实例化测试
                        GameObject instance = Instantiate(prefab);
                        instance.name = "AsyncLoadedInstance";
                        instance.transform.position = new Vector3(2, 0, 0);
                        
                        Debug.Log($"[TestResourceManager] ✅ 实例化成功: {instance.name}");
                    }
                    else
                    {
                        Debug.LogError("[TestResourceManager] ❌ 加载的对象不是 GameObject");
                    }
                },
                onLoadFailed: (error) =>
                {
                    Debug.LogError($"[TestResourceManager] ❌ 异步加载失败: {error}");
                },
                onProgress: (progress) =>
                {
                    Debug.Log($"[TestResourceManager] 加载进度: {progress * 100:F1}%");
                }
            );
        }

        /// <summary>
        /// 测试卸载
        /// </summary>
        [ContextMenu("Test Unload")]
        private void TestUnload()
        {
            Debug.Log("[TestResourceManager] 测试卸载资源...");
            CResourceManager.Instance.Unload(testPrefabPath);
            Debug.Log("[TestResourceManager] ✅ 卸载完成");
        }

        /// <summary>
        /// 测试多次加载同一资源
        /// </summary>
        [ContextMenu("Test Multiple Load")]
        private void TestMultipleLoad()
        {
            Debug.Log("[TestResourceManager] 测试多次加载同一资源...");
            
            for (int i = 0; i < 3; i++)
            {
                GameObject prefab = CResourceManager.Instance.LoadSync<GameObject>(testPrefabPath);
                if (prefab != null)
                {
                    Debug.Log($"[TestResourceManager] ✅ 第 {i + 1} 次加载成功: {prefab.name}");
                }
            }
        }

        /// <summary>
        /// 测试加载不存在的资源
        /// </summary>
        [ContextMenu("Test Load Non-Existent")]
        private void TestLoadNonExistent()
        {
            Debug.Log("[TestResourceManager] 测试加载不存在的资源...");
            
            string fakePath = "Assets/Prefabs/NonExistent.prefab";
            GameObject prefab = CResourceManager.Instance.LoadSync<GameObject>(fakePath);
            
            if (prefab == null)
            {
                Debug.Log("[TestResourceManager] ✅ 正确处理了不存在的资源");
            }
            else
            {
                Debug.LogError("[TestResourceManager] ❌ 不应该加载成功");
            }
        }
    }
}
