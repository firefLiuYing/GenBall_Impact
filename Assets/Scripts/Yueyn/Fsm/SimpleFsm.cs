using System;
using System.Collections.Generic;

namespace Yueyn.Fsm
{
    /// <summary>
    /// 轻量泛型状态机
    /// 不依赖旧 Fsm/IReference/Variable 体系，适合启动流程、UI流程等场景
    /// </summary>
    public class SimpleFsm<TContext> where TContext : class
    {
        private readonly Dictionary<Type, SimpleFsmState<TContext>> _states = new();
        private SimpleFsmState<TContext> _currentState;

        public TContext Context { get; }
        public float CurrentStateTime { get; private set; }
        public bool IsRunning => _currentState != null;
        public Type CurrentStateType => _currentState?.GetType();

        public SimpleFsm(TContext context, params SimpleFsmState<TContext>[] states)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            if (states == null || states.Length == 0)
                throw new ArgumentException("states must not be null or empty");

            foreach (var state in states)
            {
                if (state == null)
                    throw new ArgumentException("state must not be null");
                _states[state.GetType()] = state;
            }
        }

        public void Start<TState>() where TState : SimpleFsmState<TContext>
        {
            if (IsRunning)
                throw new InvalidOperationException("FSM is already running");

            if (!_states.TryGetValue(typeof(TState), out var state))
                throw new ArgumentException($"State {typeof(TState).Name} not found");

            _currentState = state;
            _currentState.OnEnter(Context);
        }

        public void ChangeState<TState>() where TState : SimpleFsmState<TContext>
        {
            if (_currentState == null)
                throw new InvalidOperationException("FSM is not running");

            if (!_states.TryGetValue(typeof(TState), out var state))
                throw new ArgumentException($"State {typeof(TState).Name} not found");

            _currentState.OnExit(Context);
            CurrentStateTime = 0f;
            _currentState = state;
            _currentState.OnEnter(Context);
        }

        public void Update(float deltaTime)
        {
            if (_currentState == null) return;
            CurrentStateTime += deltaTime;
            _currentState.OnUpdate(Context, deltaTime);
        }

        public void Shutdown()
        {
            if (_currentState != null)
            {
                _currentState.OnExit(Context);
                _currentState = null;
            }
            _states.Clear();
        }
    }

    /// <summary>
    /// 轻量状态基类
    /// </summary>
    public abstract class SimpleFsmState<TContext> where TContext : class
    {
        public virtual void OnEnter(TContext context) { }
        public virtual void OnUpdate(TContext context, float deltaTime) { }
        public virtual void OnExit(TContext context) { }
    }
}
