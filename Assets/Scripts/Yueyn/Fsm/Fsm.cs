using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Utils;

namespace Yueyn.Fsm
{
    public sealed class Fsm<T> :IFsm, IReference where T : class
    {
        private T _owner = null;
        private readonly Dictionary<Type, FsmState<T>> _states = new();
        private Dictionary<string, Variable> _datas = null;
        private FsmState<T> _currentState = null;
        private float _currentStateTime = 0;
        private bool _isDestroyed = true;
        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set => _name = value??string.Empty;
        }
        public T Owner=>_owner;
        public Type OwnerType => typeof(T);
        public bool IsRunning=>_currentState!=null;
        public bool IsDestroyed => _isDestroyed;
        public float CurrentStateTime => _currentStateTime;
        public string FullName=>new TypeNamePair(OwnerType,Name).ToString();
        public bool PrintLog { get; set; } = false;

        public void Clear()
        {
            _currentState?.OnExit(this,true);

            foreach (var state in _states.Values)
            {
                state.OnDestroy(this);
            }
            _states.Clear();
            _owner = null;
            Name=string.Empty;
            if (_datas != null)
            {
                foreach (var data in _datas.Values)
                {
                    if(data==null) continue;
                    ReferencePool.Release(data);
                }
                _datas.Clear();
            }
            _currentState = null;
            _currentStateTime = 0;
            _isDestroyed = true;
        }

        public static Fsm<T> Create(string name, T owner, params FsmState<T>[] states)
            => Create(name, owner, states.ToList());
        public static Fsm<T> Create(string name, T owner,List<FsmState<T>> states)
        {
            if (owner == null)
            {
                throw new Exception("owner is null");
            }

            if (states.IsNullOrEmpty())
            {
                throw new Exception("states is invalid");
            }

            var fsm = ReferencePool.Acquire<Fsm<T>>();
            fsm._name = name;
            fsm._owner = owner;
            fsm._isDestroyed = false;
            foreach (var state in states)
            {
                if (state == null)
                {
                    throw new Exception("state is invalid");
                }

                var stateType = state.GetType();
                if (!fsm._states.TryAdd(stateType, state))
                {
                    throw new Exception($"state {stateType} is already existed.");
                }

                state.OnInit(fsm);
            }

            return fsm;
        }
        public FsmState<T> GetState(Type stateType)
        {
            if (stateType == null)
            {
                throw new Exception("stateType is null");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new Exception("stateType is not assignable to FsmState<T>");
            }

            return _states.GetValueOrDefault(stateType);
        }
        public void Start<TState>() where TState : FsmState<T> =>Start(typeof(TState));
        public void Start(Type stateType)
        {
            if (IsRunning)
            {
                throw new Exception("fsm is already running");
            }

            if (stateType == null)
            {
                throw new Exception("stateType is null");
            }

            if (!typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new Exception("stateType is not assignable to FsmState<T>");
            }
            var state = GetState(stateType);

            _currentStateTime = 0f;
            _currentState = state ?? throw new Exception($"fsm: {this.Name} cannot find state: {stateType}");
            if (PrintLog)
            {
                Debug.Log($"{FullName}: ½øÈë £º {state.GetType().Name}");
            }
            _currentState.OnEnter(this);
        }
        public bool HasState<TState>() where TState : FsmState<T> =>_states.ContainsKey(typeof(TState));

        public bool HasState(Type stateType)
        {
            if (stateType == null)
            {
                throw new Exception("stateType is null");
            }

            if (typeof(FsmState<T>).IsAssignableFrom(stateType))
            {
                throw new Exception("stateType is not assignable to FsmState<T>");
            }
            return _states.ContainsKey(stateType);
        }
        public TState GetState<TState>() where TState : FsmState<T> =>(TState)GetState(typeof(TState));
        public FsmState<T>[] GetStates()=>_states.Values.ToArray();
        public void GetAllStates(List<FsmState<T>> results)
        {
            if (results == null)
            {
                throw new Exception("results is null");
            }
            results.Clear();
            foreach (var state in _states)
            {
                results.Add(state.Value);
            }
        }
        public bool HasData(string name)
        {
            if (name.IsNullOrEmpty())
            {
                throw new Exception("name is null or empty");
            }
            return _datas != null && _datas.ContainsKey(name);
        }
        public TData GetData<TData>(string name) where TData : Variable => (TData)GetData(name);
        public Variable GetData(string name)
        {
            if (name.IsNullOrEmpty())
            {
                throw new Exception("name is null or empty");
            }

            return _datas?.GetValueOrDefault(name);
        }

        public void SetData(string name, Variable data)
        {
            if (name.IsNullOrEmpty())
            {
                throw new Exception("name is null or empty");
            }

            if (_datas == null)
            {
                _datas = new Dictionary<string, Variable>();
            }
            var oldData=GetData(name);
            if (oldData != null)
            {
                ReferencePool.Release(data);
            }
            _datas[name] = data;
        }
        public bool RemoveData(string name)
        {
            if (name.IsNullOrEmpty())
            {
                throw new Exception("name is null or empty");
            }
            if(_datas == null) return false;
            var oldData=GetData(name);
            if (oldData != null)
            {
                ReferencePool.Release(oldData);
            }
            return _datas.Remove(name);
        }
        public void Update(float elapsedTime, float realElapseTime)
        {
            if(_currentState==null) return;
            if(IsPause) return;
            _currentStateTime += elapsedTime;
            _currentState.OnUpdate(this, elapsedTime, realElapseTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            if(_currentState==null) return;
            if(IsPause) return;
            _currentState.OnFixedUpdate(this,fixedDeltaTime);
        }
        public bool IsPause { get;private set; }
        public void SetPause(bool paused)
        {
            IsPause = paused;
        }
        public void Shutdown()=>ReferencePool.Release(this);

        internal void ChangeState<TState>() where TState : FsmState<T> =>ChangeState(typeof(TState));
        
        internal void ChangeState(Type stateType)
        {
            if (_currentState == null)
            {
                throw new Exception("Fsm is invalid");
            }
            var state = GetState(stateType);
            if (state == null)
            {
                throw new Exception($"state:{stateType} is not exist");
            }

            if (PrintLog)
            {
                Debug.Log($"{FullName}: {_currentState.GetType().Name} => {state.GetType().Name}");
            }
            _currentState.OnExit(this);
            _currentStateTime = 0f;
            _currentState = state;
            _currentState.OnEnter(this);
        }
    }
}