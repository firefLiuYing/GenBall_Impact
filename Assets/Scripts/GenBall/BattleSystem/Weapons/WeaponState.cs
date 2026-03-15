using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Character;
using GenBall.Player;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class WeaponState : MonoBehaviour,IBuffContainer,IEntity
    {
        public CharacterState Player { get;private set; }
        private IWeaponTriggerController _trigger;

        public void Init(CharacterState player)
        {
            Player = player;
            _trigger.Init(this);
        }
        public void Trigger(ButtonState buttonState)
        {
            _trigger?.Trigger(buttonState);
        }
        private void Awake()
        {
            _trigger = GetComponent<IWeaponTriggerController>();
        }

        #region Buffs

        public IReadOnlyList<IBuff> Buffs=>_buffs.ToList();
        private readonly SortedSet<IBuff> _buffs = new(new DefaultComparerBuff());
        public void AddBuff(IBuff buff)=>_buffs.Add(buff);

        public void RemoveBuff(IBuff buff)=>_buffs.Remove(buff);

        #endregion
        
        public void EntityUpdate(float deltaTime)
        {
            
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void OnRecycle()
        {
            
        }

        public void OnSpawn()
        {
            
        }
    }
}