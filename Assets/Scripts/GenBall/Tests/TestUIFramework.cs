using UnityEngine;
using Yueyn.Main;
using Yueyn.UI;
using GenBall.UI.TestForm;

namespace GenBall.Tests
{
    /// <summary>
    /// UI框架测试入口（MonoBehaviour）
    /// 挂载到场景GameObject上即可运行测试
    /// 按T键打开测试Form / P暂停 / R恢复
    /// </summary>
    public class TestUIFramework : MonoBehaviour
    {
        private void Start()
        {
            // 等待框架初始化
            Invoke(nameof(TestOpenForm), 1f);
        }

        private void Update()
        {
            // 按T键打开测试Form
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestOpenForm();
            }

            // 按P键暂停所有UI
            if (Input.GetKeyDown(KeyCode.P))
            {
                TestPauseUI();
            }

            // 按R键恢复所有UI
            if (Input.GetKeyDown(KeyCode.R))
            {
                TestResumeUI();
            }
        }

        private void TestOpenForm()
        {
            Debug.Log("=== Testing New UI Framework ===");

            // 1. 获取UI系统
            var uiSystem = SystemRepository.Instance.GetSystem<IUISystem>();
            if (uiSystem == null)
            {
                Debug.LogError("[TestUIFramework] UISystem not found! Make sure FrameworkDefault is in the scene.");
                return;
            }

            // 2. 创建Logic实例
            var logic = UILogicManager.Instance.CreateLogic<TestFormLogic>();

            // 3. 打开Form（通过Logic）
            logic.OpenFormAsync("Test Data: Hello World!");

            Debug.Log("=== Test Form Opened ===");
        }

        private void TestPauseUI()
        {
            var uiSystem = SystemRepository.Instance.GetSystem<IUISystem>();
            uiSystem?.PauseAllUI();
            Debug.Log("=== All UI Paused ===");
        }

        private void TestResumeUI()
        {
            var uiSystem = SystemRepository.Instance.GetSystem<IUISystem>();
            uiSystem?.ResumeAllUI();
            Debug.Log("=== All UI Resumed ===");
        }
    }
}
