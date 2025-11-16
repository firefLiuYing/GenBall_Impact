using System;
using Yueyn.Base.ReferencePool;

namespace Yueyn.Timer
{
    public partial class Timer
    {
        private class Event:IReference
        {
            private float _timer;
            private float _countdownTime;   // 倒计时
            private Action _callback;
            private bool _countable;    // 可数，表示有限次数重复，不可数，表示无限次数重复触发
            private int _repeatCount;   // 重复次数
            public bool NeedTrigger => _timer >= _countdownTime;
            public bool NeedRemove => _countable && _repeatCount <= 0;

            public void SetInitArgs(Action callback,float countdownTime,bool countable=false,int repeatCount=1)
            {
                _countdownTime = countdownTime;
                _callback = callback;
                _countable = countable;
                _repeatCount = repeatCount;
            }
            public void Update(float deltaTime)=>_timer += deltaTime;

            public void Trigger()
            {
                _timer = 0f;
                if (_countable) _repeatCount--;
                _callback?.Invoke();
            }

            public void Reset()=>_timer = 0f;
            public void Clear()
            {
                _timer = 0f;
                _callback = null;
                _countable = false;
            }
        }
    }
}