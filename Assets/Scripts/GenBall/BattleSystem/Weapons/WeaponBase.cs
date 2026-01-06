using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Accessory;
using GenBall.BattleSystem.Generated;
using GenBall.Player;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.EventPool;
using Yueyn.Event;

namespace GenBall.BattleSystem.Weapons
{
    public abstract class WeaponBase : MonoBehaviour,IWeapon
    {
        private readonly EventPool<GameEventArgs> _eventPool = new(EventPoolMode.AllowNoHandler|EventPoolMode.AllowMultiHandler);
        private readonly List<IEffect> _effects = new();
        private readonly Dictionary<Type, IWeaponComponent> _weaponComponents = new();
        public void EntityUpdate(float deltaTime)
        {
            this.FireNowSystemUpdate(deltaTime);
            _eventPool.Update(deltaTime,deltaTime);
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


        public IAttacker Owner { get;private set; }
        public void Trigger(ButtonState triggerState)
        {
            this.FireNowInputTrigger(triggerState);
            OnTrigger(triggerState);
        }
        protected virtual void OnTrigger(ButtonState triggerState){}

        public void Equip(IAttacker owner)
        {
            Owner=owner;
            EquipWeaponComponent();
            OnEquip(owner);
            gameObject.SetActive(true);
        }
        protected virtual void OnEquip(IAttacker attacker){}

        public void Unequip()
        {
            foreach (var effect in _effects)
            {
                effect.Unapply();
            }
            UnequipWeaponComponents();
            Owner = null;
        }

        public void Attack(IAttackable target, AttackInfo attackInfo)
        {
            this.FireNowCombatBeforeAttackJustify(target,attackInfo);
            var result = BattleController.Attack(target, attackInfo);
            this.FireNowCombatAfterAttackCalculate(result);
        }

        public T GetWeaponComponent<T>() where T : IWeaponComponent =>(T)InternalGetWeaponComponent(typeof(T));
        private IWeaponComponent InternalGetWeaponComponent([NotNull] Type type) => _weaponComponents.GetValueOrDefault(type);

        private void EquipWeaponComponent()
        {
            _weaponComponents.Clear();
            var weaponComponents = GetComponentsInChildren<IWeaponComponent>();
            foreach (var weaponComponent in weaponComponents)
            {
                _weaponComponents.Add(weaponComponent.GetType(),weaponComponent);
            }

            foreach (var wc in _weaponComponents.Values)
            {
                wc.Equip(this);
            }
        }

        private void UnequipWeaponComponents()
        {
            foreach (var weaponComponent in _weaponComponents.Values)
            {
                weaponComponent.Unequip();
            }
            _weaponComponents.Clear();
        }
        public abstract IWeaponStats Stats { get; }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)=>_eventPool.Subscribe(id, handler);

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)=>_eventPool.Unsubscribe(id, handler);

        public void FireEvent(object sender, GameEventArgs e)=>_eventPool.Fire(sender, e);

        public void FireNow(object sender, GameEventArgs e)=>_eventPool.FireNow(sender, e);
    }
}