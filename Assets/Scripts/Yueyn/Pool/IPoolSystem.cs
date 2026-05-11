using System;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace Yueyn.Pool
{
    /// <summary>
    /// 对象池系统接口，统一管理 C# 引用池和 Unity 对象池
    /// </summary>
    public interface IPoolSystem : ISystem
    {
        #region C# 引用池 (对应原 ReferencePool)

        /// <summary>从引用池获取对象（无可用则 new）</summary>
        T Acquire<T>() where T : class, IReference, new();

        /// <summary>从引用池获取对象（运行时类型）</summary>
        IReference Acquire(Type type);

        /// <summary>归还对象到引用池（会调用 Clear() 重置状态）</summary>
        void Release(IReference obj);

        /// <summary>预创建指定数量的对象放入池中</summary>
        void PreCreate<T>(int count) where T : class, IReference, new();

        /// <summary>获取指定类型正在使用中的对象数量</summary>
        int GetUsingCount(Type type);

        /// <summary>获取指定类型空闲可用的对象数量</summary>
        int GetUnusedCount(Type type);

        /// <summary>清空指定类型的所有缓存对象</summary>
        void RemoveAll(Type type);

        /// <summary>清空所有缓存</summary>
        void ClearAll();

        #endregion
    }
}
