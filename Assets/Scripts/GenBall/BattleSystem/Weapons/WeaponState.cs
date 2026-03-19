using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.Player;
using GenBall.Procedure.Game;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class WeaponState : MonoBehaviour,IBuffContainer,IEntity
    {
        public CharacterState Player { get;private set; }
        private IWeaponTriggerController _trigger;
        private IWeaponReloadController _reload;

        private readonly List<AccessoryObj> _accessoryObjs = new();

        public void AddAccessory(AccessoryObj accessoryObj)
        {
            _accessoryObjs.Add(accessoryObj);
            accessoryObj.OnAdd(this);
        }

        private void RemoveAllAccessories()
        {
            foreach (var accessoryObj in _accessoryObjs)
            {
                accessoryObj.OnRemove();
            }
            _accessoryObjs.Clear();
        }

        public void OnUnequip()
        {
            RemoveAllAccessories();    
        }
        public void Init(CharacterState player)
        {
            Player = player;
            _trigger.Init(this);
            _reload.Init(this);
        }
        public void Trigger(ButtonState buttonState)
        {
            _trigger?.Trigger(buttonState);
        }

        public void Reload(ButtonState buttonState)
        {
            _reload?.Reload(buttonState);
        }
        private void Awake()
        {
            _trigger = GetComponent<IWeaponTriggerController>();
            _reload = GetComponent<IWeaponReloadController>();
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