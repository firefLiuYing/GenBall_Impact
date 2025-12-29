using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Accessory;
using GenBall.BattleSystem.Generated;
using GenBall.Player;
using UnityEngine;
using Yueyn.Base.EventPool;

namespace GenBall.BattleSystem.Weapons
{
    public abstract class WeaponBase : MonoBehaviour,IWeapon
    {
        private readonly EventPool<EffectEventArgs> _eventPool = new(EventPoolMode.AllowNoHandler|EventPoolMode.AllowMultiHandler);
        private readonly List<IEffect> _effects = new();
        public void EntityUpdate(float deltaTime)
        {
            this.FireUpdate(deltaTime);
            OnUpdate(deltaTime);
        }
        protected virtual void OnUpdate(float deltaTime){}

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            this.FireFixedUpdate(fixedDeltaTime);
            OnFixedUpdate(fixedDeltaTime);
        }

        protected virtual void OnFixedUpdate(float fixedDeltaTime){}

        public void OnRecycle()
        {
            _eventPool.Clear();
            _effects.Clear();
        }

        public void AddEffect(IEffect effect)
        {
            _effects.Add(effect);
            effect.Apply(this);
        }

        public bool RemoveEffect(IEffect effect)
        {
            if (!_effects.Remove(effect)) return false;
            effect.Unapply();
            return true;
        }

        public void Subscribe(int id, EventHandler<EffectEventArgs> handler)=>_eventPool.Subscribe(id, handler);

        public void Unsubscribe(int id, EventHandler<EffectEventArgs> handler)=>_eventPool.Unsubscribe(id, handler);
        
        public void FireEvent(object sender,EffectEventArgs e)=>_eventPool.Fire(sender,e);
        
        public void FireEventNow(object sender,EffectEventArgs e)=>_eventPool.Fire(sender,e);

        public IAttacker Owner { get;private set; }
        public void Trigger(ButtonState triggerState)
        {
            this.FireTrigger(triggerState);
            OnTrigger(triggerState);
        }
        protected virtual void OnTrigger(ButtonState triggerState){}

        public void Equip(IAttacker owner)
        {
            Owner=owner;
            OnEquip(owner);
            gameObject.SetActive(true);
        }
        protected virtual void OnEquip(IAttacker attacker){}

        public void Unequip()
        {
            Owner = null;
        }

        public void Attack(IAttackable target, AttackInfo attackInfo)
        {
            this.FireBeforeAttackJustify(target,attackInfo);
            var result = BattleController.Attack(target, attackInfo);
            this.FireAfterAttackCalculate(result);
        }

        public abstract IWeaponStats Stats { get; }
    }
}