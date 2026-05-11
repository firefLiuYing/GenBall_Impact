using System;
using UnityEngine;
using Yueyn.Main;
using Yueyn.Event;

namespace GenBall.Tests
{
    /// <summary>
    /// 事件系统测试入口（MonoBehaviour）
    /// 挂载到场景GameObject上即可运行全部测试
    /// 按E键手动触发
    /// </summary>
    public class TestEventSystem : MonoBehaviour
    {
        private int _passCount;
        private int _failCount;
        private CEventSystem _globalChannel;
        private bool _defaultHandlerCalled;

        private void Start()
        {
            Invoke(nameof(RunAllTests), 1f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                RunAllTests();
            }
        }

        private void RunAllTests()
        {
            Debug.Log("========================================");
            Debug.Log("  CEventSystem 测试开始");
            Debug.Log("========================================");

            _passCount = 0;
            _failCount = 0;

            _globalChannel = SystemRepository.Instance.GetSystem<IEventSystem>() as CEventSystem;
            if (_globalChannel == null)
            {
                Debug.LogError("[TestEventSystem] 全局 IEventSystem 未注册！请确保 FrameworkDefault 已在场景中");
                return;
            }

            TestBasicSubscribeFireUnsubscribe();
            TestGenericParams();
            TestDeferredFire();
            TestGlobalChannel();
            TestDefaultHandler();
            TestCheckMethod();
            TestClearPendingQueue();
            TestMultipleHandlers();
            TestUnsubscribeExceptionSafe();

            Debug.Log("========================================");
            Debug.Log($"  测试完成: 通过 {_passCount}/{_passCount + _failCount}, 失败 {_failCount}");
            Debug.Log("========================================");
        }

        #region 测试1: 基础 Subscribe / FireNow / Unsubscribe

        private void TestBasicSubscribeFireUnsubscribe()
        {
            var ch = new CEventSystem();
            bool fired = false;
            Action handler = () => fired = true;

            ch.Subscribe(1, handler);
            ch.FireNow(1);
            Assert("基础 FireNow", fired == true);

            ch.Unsubscribe(1, handler);
            fired = false;
            ch.FireNow(1);
            Assert("基础 Unsubscribe 后不再触发", fired == false);

            ch.UnInit();
        }

        #endregion

        #region 测试2: 多参数泛型 1~4 个参数

        private void TestGenericParams()
        {
            var ch = new CEventSystem();

            string result = "";

            // 1 参数
            ch.Subscribe<int>(10, a => result += $"[{a}]");
            ch.FireNow<int>(10, 100);
            Assert("1参数", result == "[100]");

            // 2 参数
            result = "";
            ch.Subscribe<int, string>(11, (a, b) => result += $"[{a}:{b}]");
            ch.FireNow<int, string>(11, 200, "OK");
            Assert("2参数", result == "[200:OK]");

            // 3 参数
            result = "";
            ch.Subscribe<int, string, float>(12, (a, b, c) => result += $"[{a}:{b}:{c:F1}]");
            ch.FireNow<int, string, float>(12, 300, "HI", 1.5f);
            Assert("3参数", result == "[300:HI:1.5]");

            // 4 参数
            result = "";
            ch.Subscribe<int, string, float, bool>(13, (a, b, c, d) => result += $"[{a}:{b}:{c:F1}:{d}]");
            ch.FireNow<int, string, float, bool>(13, 400, "Q", 2.7f, true);
            Assert("4参数", result == "[400:Q:2.7:True]");

            ch.UnInit();
        }

        #endregion

        #region 测试3: Fire 延迟触发

        private void TestDeferredFire()
        {
            var ch = new CEventSystem();
            bool fired = false;

            ch.Subscribe(20, () => fired = true);

            // Fire 延迟入队，此时不应触发
            ch.Fire(20);
            Assert("Fire 延迟 - 立即未触发", fired == false);

            // 手动 RenderUpdate 触发延迟队列
            ch.RenderUpdate(0.016f);
            Assert("Fire 延迟 - RenderUpdate后触发", fired == true);

            ch.UnInit();
        }

        #endregion

        #region 测试4: 全局 IEventSystem

        private void TestGlobalChannel()
        {
            bool fired = false;
            Action handler = () => fired = true;

            _globalChannel.Subscribe(9999, handler);
            _globalChannel.FireNow(9999);
            Assert("全局 IEventSystem 订阅/触发正常", fired == true);

            _globalChannel.Unsubscribe(9999, handler);
        }

        #endregion

        #region 测试5: DefaultHandler

        private void TestDefaultHandler()
        {
            var ch = new CEventSystem();
            _defaultHandlerCalled = false;

            ch.SetDefaultHandler(id =>
            {
                if (id == 30) _defaultHandlerCalled = true;
            });

            // 无订阅者 -> 调用 DefaultHandler
            ch.FireNow(30);
            Assert("DefaultHandler 无订阅者时调用", _defaultHandlerCalled == true);

            // 有订阅者 -> 不调用 DefaultHandler
            _defaultHandlerCalled = false;
            ch.Subscribe(30, () => { });
            ch.FireNow(30);
            Assert("DefaultHandler 有订阅者时不调用", _defaultHandlerCalled == false);

            ch.UnInit();
        }

        #endregion

        #region 测试6: Check 方法

        private void TestCheckMethod()
        {
            var ch = new CEventSystem();
            Action handler1 = () => { };
            Action handler2 = () => { };

            ch.Subscribe(40, handler1);

            Assert("Check 已订阅 handler", ch.Check(40, handler1) == true);
            Assert("Check 未订阅 handler", ch.Check(40, handler2) == false);
            Assert("Check 不存在的 channelId", ch.Check(99, handler1) == false);

            ch.UnInit();
        }

        #endregion

        #region 测试7: Clear 延迟队列

        private void TestClearPendingQueue()
        {
            var ch = new CEventSystem();
            bool fired = false;

            ch.Subscribe(50, () => fired = true);
            ch.Fire(50);
            ch.Clear();

            // 清空队列后 RenderUpdate 不应触发
            ch.RenderUpdate(0.016f);
            Assert("Clear 延迟队列 - RenderUpdate 不触发", fired == false);

            ch.UnInit();
        }

        #endregion

        #region 测试8: 同一 ID 多个 Handler

        private void TestMultipleHandlers()
        {
            var ch = new CEventSystem();
            var order = new System.Collections.Generic.List<string>();

            ch.Subscribe(60, () => order.Add("A"));
            ch.Subscribe(60, () => order.Add("B"));
            ch.Subscribe(60, () => order.Add("C"));
            ch.FireNow(60);

            Assert("多 Handler 全部触发", order.Count == 3);
            Assert("多 Handler 调用顺序", order[0] == "A" && order[1] == "B" && order[2] == "C");

            ch.UnInit();
        }

        #endregion

        #region 测试9: Unsubscribe 异常安全

        private void TestUnsubscribeExceptionSafe()
        {
            var ch = new CEventSystem();
            Action existingHandler = () => { };
            Action nonExistentHandler = () => { };

            ch.Subscribe(70, existingHandler);

            bool noError1 = true;
            try { ch.Unsubscribe(70, existingHandler); }
            catch { noError1 = false; }
            Assert("Unsubscribe 存在的 handler 不异常", noError1 == true);

            bool noError2 = true;
            try { ch.Unsubscribe(70, nonExistentHandler); }
            catch { noError2 = false; }
            Assert("Unsubscribe 不存在的 handler 不异常", noError2 == true);

            bool noError3 = true;
            try { ch.Unsubscribe(8888, nonExistentHandler); }
            catch { noError3 = false; }
            Assert("Unsubscribe 不存在的 channelId 不异常", noError3 == true);

            ch.UnInit();
        }

        #endregion

        #region 辅助方法

        private void Assert(string testName, bool condition)
        {
            if (condition)
            {
                _passCount++;
                Debug.Log($"  [PASS] {testName}");
            }
            else
            {
                _failCount++;
                Debug.LogError($"  [FAIL] {testName}");
            }
        }

        #endregion
    }
}
