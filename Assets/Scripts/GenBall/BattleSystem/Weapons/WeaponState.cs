using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.Framework.Entity;
using GenBall.Player;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Weapons
{
    [Obsolete]
    public class WeaponState : MonoBehaviour,IBuffContainer,IEntityLogicUpdate
    {
        public GameObject PlayerGo { get; private set; }
        private IWeaponTriggerController _trigger;
        private IWeaponReloadController _reload;
        
        private readonly List<AccessoryObj> _accessoryObjs = new();
        [SerializeField] private WeaponModel model;
        public WeaponStats Stats { get;private set; }
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
        public void Init(GameObject playerGo)
        {
            PlayerGo = playerGo;
            Stats = new WeaponStats
            {
                Damage = DamageValue.Create(model.damage),
                FireInterval = FloatStat.Create(model.fireInterval),
                ReloadTime = FloatStat.Create(model.reloadTime),
            };
            _trigger.Init(this);
            _reload.Init(this);
            SystemRepository.Instance.GetSystem<IEntityUpdateSystem>().AddLogicUpdate(this);
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
        
        public void LogicUpdate(float deltaTime)
        {
        }

        private void OnDestroy()
        {
            SystemRepository.Instance.GetSystem<IEntityUpdateSystem>()?.RemoveLogicUpdate(this);
        }
    }

    [Serializable]
    public struct WeaponModel
    {
        public int damage;
        public float fireInterval;
        public float reloadTime;
    }
}