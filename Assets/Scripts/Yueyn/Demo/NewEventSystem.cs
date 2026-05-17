using System;
using System.Collections.Generic;
using Yueyn.Utils;

namespace Yueyn.Demo.Event
{
    /// <summary>
    /// 全局事件总线
    /// </summary>
    public class EventRouter:Singleton<EventRouter>
    {
        // 实际是借助EventDispatcher来实现的，这里只是一个简单的封装
        private EventDispatcher _dispatcher=new();
        protected override void Init()
        {
            
        }
        public void Subscribe(int eventId,Action action){}
        public void Unsubscribe(int eventId,Action action){}
        public void BoardCast(int eventId){}
        
        public void Subscribe<T1>(int eventId,Action<T1> action){}
        public void Unsubscribe<T1>(int eventId,Action<T1> action){}
        public void BoardCast<T1>(int eventId,T1 data){}
        
        // 一直到四个泛型参数
    }
    
    /// <summary>
    /// 实际的事件分发器，用于分发事件，可以给全局事件总线使用，也可以给其他系统使用，构造自己单独的事件总线
    /// </summary>
    public class EventDispatcher
    {
        private Dictionary<int,EventHandlerGroup> _eventHandlerGroups=new();
        public void Subscribe(int eventId,Action action){}
        public void Unsubscribe(int eventId,Action action){}
        public void BoardCast(int eventId){}

        public void Subscribe<T1>(int eventId, Action<T1> action)
        {
            
        }
        public void Unsubscribe<T1>(int eventId,Action<T1> action){}
        public void BoardCast<T1>(int eventId,T1 data){}
        
        // 一直到四个泛型参数
    }

    // 事件处理器组，用于存储同一个事件的不同处理器
    public class EventHandlerGroup
    {
        // 这个类和EventHandler两个之间的具体实现我还没想好，总之可以借助Delegate来避免反复构造EventArgs
        public int EventId;
        private SafeIterableList<EventHandler> _handlers=new();

        public void AddHandler(Action action)
        {
            
        }
        public void AddHandler<T1>(Action<T1> action)
        {
            
        }
        // 略
    }

    // 事件处理器，用于存储单个事件的处理器
    public class EventHandler
    {
        public Delegate Handler;
    }
}