using System;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;
using Yueyn.Pool;

namespace GenBall.Tests
{
    /// <summary>
    /// 对象池系统 IPoolSystem 测试入口（MonoBehaviour）
    /// 挂载到场景GameObject上即可运行全部测试
    /// 按E键手动触发重新运行
    /// </summary>
    public class TestPoolSystem : MonoBehaviour
    {
        private int _passCount;
        private int _failCount;
        private IPoolSystem _poolSystem;

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
            Debug.Log("  IPoolSystem 测试开始");
            Debug.Log("========================================");

            _passCount = 0;
            _failCount = 0;

            _poolSystem = SystemRepository.Instance.GetSystem<IPoolSystem>();
            if (_poolSystem == null)
            {
                Debug.LogError("[TestPoolSystem] IPoolSystem 未注册！请确保 FrameworkDefault 已在场景中");
                return;
            }

            // 先全局清空，避免残留影响
            _poolSystem.ClearAll();

            TestAcquireNewObject();
            TestAcquireReleaseReuse();
            TestMultipleAcquires();
            TestReleaseResetsState();
            TestUsingCount();
            TestPreCreate();
            TestRemoveAll();
            TestClearAll();
            TestAcquireByType();
            TestNullSafety();

            Debug.Log("========================================");
            Debug.Log($"  测试完成: 通过 {_passCount}/{_passCount + _failCount}, 失败 {_failCount}");
            Debug.Log("========================================");
        }

        #region 测试1: Acquire 新对象

        private void TestAcquireNewObject()
        {
            // 隔离：确保该类型池为空
            _poolSystem.RemoveAll(typeof(TestPoolObject));

            var obj1 = _poolSystem.Acquire<TestPoolObject>();
            var obj2 = _poolSystem.Acquire<TestPoolObject>();

            Assert("Acquire 返回非空", obj1 != null && obj2 != null);
            Assert("Acquire 不同实例", !ReferenceEquals(obj1, obj2));

            _poolSystem.Release(obj1);
            _poolSystem.Release(obj2);
        }

        #endregion

        #region 测试2: Release 后复用

        private void TestAcquireReleaseReuse()
        {
            // 隔离：确保该类型池为空
            _poolSystem.RemoveAll(typeof(TestPoolObject));

            var obj1 = _poolSystem.Acquire<TestPoolObject>();
            obj1.Value = 42;
            obj1.Name = "Original";

            // Release 会调用 Clear() 重置并放回池
            _poolSystem.Release(obj1);

            // 池中仅有 obj1 一个对象，再次 Acquire 必然复用它
            var obj2 = _poolSystem.Acquire<TestPoolObject>();
            Assert("Release 后 Acquire 复用同一对象", ReferenceEquals(obj1, obj2));
            Assert("Release 后 Clear 重置了 Value", obj2.Value == 0);
            Assert("Release 后 Clear 重置了 Name", obj2.Name == null);

            _poolSystem.Release(obj2);
        }

        #endregion

        #region 测试3: 多次获取释放

        private void TestMultipleAcquires()
        {
            // 隔离
            _poolSystem.RemoveAll(typeof(TestPoolObject));

            var objs = new TestPoolObject[5];
            for (int i = 0; i < 5; i++)
            {
                objs[i] = _poolSystem.Acquire<TestPoolObject>();
                objs[i].Value = i + 1;
            }

            Assert("多次 Acquire 都成功", objs[4] != null);

            for (int i = 0; i < 5; i++)
            {
                _poolSystem.Release(objs[i]);
            }
        }

        #endregion

        #region 测试4: Release 触发 Clear

        private void TestReleaseResetsState()
        {
            // 隔离
            _poolSystem.RemoveAll(typeof(TestPoolObject));

            var obj = _poolSystem.Acquire<TestPoolObject>();
            obj.Value = 999;
            obj.Name = "TestName";
            obj.Flag = true;

            _poolSystem.Release(obj);

            Assert("Clear 重置 Value", obj.Value == 0);
            Assert("Clear 重置 Name", obj.Name == null);
            Assert("Clear 重置 Flag", obj.Flag == false);
        }

        #endregion

        #region 测试5: UsingCount 统计

        private void TestUsingCount()
        {
            Type type = typeof(TestPoolObject2);
            _poolSystem.RemoveAll(type); // 确保干净起始

            int initial = _poolSystem.GetUsingCount(type);
            Assert("初始 UsingCount 为 0", initial == 0);

            var obj1 = _poolSystem.Acquire<TestPoolObject2>();
            Assert("Acquire 后 UsingCount=1", _poolSystem.GetUsingCount(type) == 1);

            var obj2 = _poolSystem.Acquire<TestPoolObject2>();
            Assert("再次 Acquire 后 UsingCount=2", _poolSystem.GetUsingCount(type) == 2);

            _poolSystem.Release(obj1);
            Assert("Release 一个后 UsingCount=1", _poolSystem.GetUsingCount(type) == 1);

            _poolSystem.Release(obj2);
            Assert("全部 Release 后 UsingCount=0", _poolSystem.GetUsingCount(type) == 0);
        }

        #endregion

        #region 测试6: PreCreate 预创建

        private void TestPreCreate()
        {
            Type type = typeof(TestPoolObject3);
            _poolSystem.RemoveAll(type);

            _poolSystem.PreCreate<TestPoolObject3>(3);
            // PreCreate 只是往池里放，不影响 UsingCount
            Assert("PreCreate 后 UsingCount 仍为 0", _poolSystem.GetUsingCount(type) == 0);

            // 预创建后再 Acquire 应该能拿到预创建的对象
            var obj1 = _poolSystem.Acquire<TestPoolObject3>();
            var obj2 = _poolSystem.Acquire<TestPoolObject3>();
            var obj3 = _poolSystem.Acquire<TestPoolObject3>();
            Assert("预创建3个可正常取出3个", obj1 != null && obj2 != null && obj3 != null);

            _poolSystem.Release(obj1);
            _poolSystem.Release(obj2);
            _poolSystem.Release(obj3);
        }

        #endregion

        #region 测试7: RemoveAll 指定类型

        private void TestRemoveAll()
        {
            Type type = typeof(TestPoolObject4);
            _poolSystem.RemoveAll(type);

            var obj1 = _poolSystem.Acquire<TestPoolObject4>();
            _poolSystem.Release(obj1); // 放回池中

            _poolSystem.RemoveAll(type);

            // RemoveAll 后再 Acquire 应该是新对象（旧的被清掉了）
            var obj2 = _poolSystem.Acquire<TestPoolObject4>();
            Assert("RemoveAll 后 Acquire 得到新对象", !ReferenceEquals(obj1, obj2));

            _poolSystem.Release(obj2);
        }

        #endregion

        #region 测试8: ClearAll 全清

        private void TestClearAll()
        {
            // 用独立类型，不受其他测试影响
            Type typeA = typeof(TestPoolObject5);
            Type typeB = typeof(TestPoolObject6);
            _poolSystem.RemoveAll(typeA);
            _poolSystem.RemoveAll(typeB);

            var a = _poolSystem.Acquire<TestPoolObject5>();
            var b = _poolSystem.Acquire<TestPoolObject6>();
            _poolSystem.Release(a);
            _poolSystem.Release(b);

            _poolSystem.ClearAll();

            // ClearAll 之后，之前释放的对象应该都被清除
            var a2 = _poolSystem.Acquire<TestPoolObject5>();
            var b2 = _poolSystem.Acquire<TestPoolObject6>();
            Assert("ClearAll 后 类型5是新对象", !ReferenceEquals(a, a2));
            Assert("ClearAll 后 类型6是新对象", !ReferenceEquals(b, b2));

            _poolSystem.Release(a2);
            _poolSystem.Release(b2);
        }

        #endregion

        #region 测试9: 按运行时Type获取

        private void TestAcquireByType()
        {
            Type type = typeof(TestPoolObject);
            _poolSystem.RemoveAll(type);

            var obj = _poolSystem.Acquire(type);
            Assert("按 Type Acquire 非空", obj != null);
            Assert("按 Type Acquire 正确类型", obj is TestPoolObject);

            _poolSystem.Release(obj);
        }

        #endregion

        #region 测试10: 空值安全

        private void TestNullSafety()
        {
            bool noError = true;
            try { _poolSystem.Release(null); }
            catch { noError = false; }
            Assert("Release null 不抛异常", noError == true);
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

        #region 测试用 IReference 对象

        private class TestPoolObject : IReference
        {
            public int Value;
            public string Name;
            public bool Flag;

            public void Clear()
            {
                Value = 0;
                Name = null;
                Flag = false;
            }
        }

        private class TestPoolObject2 : IReference
        {
            public int Value;
            public void Clear() => Value = 0;
        }

        private class TestPoolObject3 : IReference
        {
            public void Clear() { }
        }

        private class TestPoolObject4 : IReference
        {
            public int Id;
            public void Clear() => Id = 0;
        }

        private class TestPoolObject5 : IReference
        {
            public void Clear() { }
        }

        private class TestPoolObject6 : IReference
        {
            public void Clear() { }
        }

        #endregion
    }
}
