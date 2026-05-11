using System;

namespace Yueyn.Event
{
    /// <summary>
    /// 事件通道接口，框架层只认 int 事件ID。
    /// 业务层可自行定义 enum 并转为 int 使用。
    /// 同一个实现类即可作为全局通道（通过 SystemRepository 注册），
    /// 也可作为局部通道（由业务对象自行持有实例）。
    /// </summary>
    public interface IEventSystem : Yueyn.Main.ISystem
    {
        void Subscribe(int channelId, Action handler);
        void Subscribe<T1>(int channelId, Action<T1> handler);
        void Subscribe<T1, T2>(int channelId, Action<T1, T2> handler);
        void Subscribe<T1, T2, T3>(int channelId, Action<T1, T2, T3> handler);
        void Subscribe<T1, T2, T3, T4>(int channelId, Action<T1, T2, T3, T4> handler);

        void Unsubscribe(int channelId, Action handler);
        void Unsubscribe<T1>(int channelId, Action<T1> handler);
        void Unsubscribe<T1, T2>(int channelId, Action<T1, T2> handler);
        void Unsubscribe<T1, T2, T3>(int channelId, Action<T1, T2, T3> handler);
        void Unsubscribe<T1, T2, T3, T4>(int channelId, Action<T1, T2, T3, T4> handler);

        void Fire(int channelId);
        void Fire<T1>(int channelId, T1 arg1);
        void Fire<T1, T2>(int channelId, T1 arg1, T2 arg2);
        void Fire<T1, T2, T3>(int channelId, T1 arg1, T2 arg2, T3 arg3);
        void Fire<T1, T2, T3, T4>(int channelId, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        void FireNow(int channelId);
        void FireNow<T1>(int channelId, T1 arg1);
        void FireNow<T1, T2>(int channelId, T1 arg1, T2 arg2);
        void FireNow<T1, T2, T3>(int channelId, T1 arg1, T2 arg2, T3 arg3);
        void FireNow<T1, T2, T3, T4>(int channelId, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        bool Check(int channelId, Delegate handler);
        void SetDefaultHandler(Action<int> handler);
        void Clear();
    }
}
