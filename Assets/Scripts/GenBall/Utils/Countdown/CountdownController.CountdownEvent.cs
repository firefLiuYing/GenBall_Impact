using System;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;

namespace GenBall.Utils.Countdown
{
    public partial class CountdownController
    {
        private class CountdownEvent : IReference
        {
            private float _countdownTime;
            /// <summary>
            /// 传入参数为剩余冷却时间
            /// </summary>
            private Action<float> _updateCallback;
            private Action _completeCallback;

            private float _timer;
            
            private bool _hasTrigger;
            private bool _paused;
            
            public bool Compeleted => _timer>=_countdownTime;

            /// <summary>
            /// 重新设置倒计时时长，效果等于新建，继承原有callback
            /// </summary>
            /// <param name="newCountdownTime"></param>
            public void ResetCountdown(float newCountdownTime)
            {
                _countdownTime=newCountdownTime;
                _timer = _countdownTime + 0.01f;
                _hasTrigger = true;
                _paused = false;
            }
            
            public void Pause()
            {
                _paused = true;
            }

            public void Resume()
            {
                _paused = false;
            }

            public void Start()
            {
                _timer = 0;
                _hasTrigger=false;
                _paused = false;
            }
            public void Update(float deltaTime)
            {
                if(_paused) return;
                if(_hasTrigger) return;
                if (_timer >= _countdownTime)
                {
                    _hasTrigger = true;
                    _completeCallback?.Invoke();
                }
                _timer += deltaTime;
                _updateCallback?.Invoke(_countdownTime-_timer);
            }
            
            /// <summary>
            /// 创建时是已经完成计时的状态，需要手动开始计时
            /// </summary>
            /// <param name="countdownTime"></param>
            /// <param name="updateCallback"></param>
            /// <param name="completeCallback"></param>
            /// <returns></returns>
            public static CountdownEvent Create(float countdownTime, Action<float> updateCallback=null, Action completeCallback=null)
            {
                var countdownEvent = ReferencePool.Acquire<CountdownEvent>();
                countdownEvent._countdownTime = countdownTime;
                countdownEvent._updateCallback = updateCallback;
                countdownEvent._completeCallback = completeCallback;
                countdownEvent._timer = countdownTime+0.01f;
                countdownEvent._hasTrigger = true;
                countdownEvent._paused = false;
                return countdownEvent;
            }

            public void Clear()
            {
                _countdownTime = 0;
                _timer = 0;
                _updateCallback = null;
                _completeCallback = null;
                _hasTrigger = false;
                _paused = false;
            }
        }
    }
}