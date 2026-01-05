using System;
using System.Collections.Generic;
using GenBall.BattleSystem;
using GenBall.BattleSystem.Generated;
using GenBall.Enemy.Fsm;
using GenBall.Utils.EntityCreator;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.EventPool;
using Yueyn.Event;

namespace GenBall.Enemy
{
    public abstract class EnemyBase : MonoBehaviour,IEnemy,IAttacker
    {
        private readonly List<Module> _moduleMap = new();
        // private FsmModule _fsmModule;
        private readonly EventPool<GameEventArgs>  _eventPool=new (EventPoolMode.AllowNoHandler|EventPoolMode.AllowMultiHandler);
        private readonly List<IEffect> _effects = new();
        private Module GetModule(Type type)
        {
            foreach (var module in _moduleMap)
            {
                if (type.IsAssignableFrom(module.GetType()))
                {
                    return module;
                }
            }
            throw new Exception($"Module:{type} not found");
        }
        public Player.Player Target { get;private set; }
        public void SetTarget(Player.Player target)=>Target = target;
        public TModule GetModule<TModule>() where TModule : Module =>(TModule)GetModule(typeof(TModule));
        // public AttackResult OnAttacked(AttackInfo attackInfo)=>_fsmModule?.OnAttacked(attackInfo)??AttackResult.Create(0,false);

        public abstract AttackResult OnAttacked(AttackInfo attackInfo);

        public void EntityUpdate(float deltaTime)
        {
            this.FireNowSystemUpdate(deltaTime);
            OnUpdate(deltaTime);
        }
        protected virtual void OnUpdate(float deltaTime){}

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            this.FireNowSystemFixedUpdate(fixedDeltaTime);
            OnFixedUpdate(fixedDeltaTime);
        }
        protected virtual void OnFixedUpdate(float fixedDeltaTime){}

        public void OnRecycle()
        {
            foreach (var module in _moduleMap)
            {
                module.OnRecycle();
            }
            _moduleMap.Clear();
            gameObject.SetActive(false);
        }

        public void Initialize()
        {
            _moduleMap.Clear();
            var modules = GetComponentsInChildren<Module>();
            foreach (var module in modules)
            {
                module.SetOwner(this);
                _moduleMap.Add(module);
            }
            foreach (var module in modules)
            {
                module.Initialize();
            }
            // _fsmModule=GetModule<FsmModule>();
            
            gameObject.SetActive(true);
        }

        public void Death()
        {
            // _fsmModule.OnDeath();
            GameEntry.GetModule<EntityCreator<IEnemy>>().RecycleEntity(gameObject);
        }

        public void AddEffect(IEffect effect)
        {
            _effects.Add(effect);
            effect.Apply(this);
        }

        public bool RemoveEffect(IEffect effect)
        {
            if(!_effects.Remove(effect)) return false;
            effect.Unapply();
            return true;
        }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler) => _eventPool.Subscribe(id, handler);

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)=>_eventPool.Unsubscribe(id, handler);

        public void FireEvent(object sender, GameEventArgs e)=>_eventPool.Fire(sender, e);

        public void FireNow(object sender, GameEventArgs e)=>_eventPool.FireNow(sender, e);
    }
}