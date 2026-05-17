using UnityEngine;

namespace GenBall.Test
{
    /// <summary>
    /// 创建测试预制体的辅助脚本
    /// 在 Unity 编辑器中运行此脚本创建测试用的 Cube 预制体
    /// </summary>
    public class CreateTestPrefab : MonoBehaviour
    {
        [ContextMenu("Create Test Cube Prefab")]
        private void CreateTestCube()
        {
            #if UNITY_EDITOR
            // 创建一个 Cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "TestCube";
            
            // 添加一个简单的组件
            cube.AddComponent<TestCubeComponent>();
            
            // 保存为预制体
            string prefabPath = "Assets/Prefabs/TestCube.prefab";
            
            // 确保目录存在
            string directory = System.IO.Path.GetDirectoryName(prefabPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // 创建预制体
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(cube, prefabPath);
            
            // 删除场景中的临时对象
            DestroyImmediate(cube);
            
            Debug.Log($"✅ 测试预制体已创建: {prefabPath}");
            UnityEditor.AssetDatabase.Refresh();
            #else
            Debug.LogWarning("此功能仅在编辑器中可用");
            #endif
        }
    }

    /// <summary>
    /// 测试 Cube 组件
    /// </summary>
    public class TestCubeComponent : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log($"[TestCubeComponent] {gameObject.name} 已实例化");
        }

        private void Update()
        {
            // 简单的旋转动画
            transform.Rotate(Vector3.up, 30f * Time.deltaTime);
        }
    }
}
