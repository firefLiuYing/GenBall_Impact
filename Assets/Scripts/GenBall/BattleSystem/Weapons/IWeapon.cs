using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public interface IWeapon:IEffectable
    {
        public IAttacker Owner { get; }
        public void Trigger(ButtonState triggerState);
        public void Equip(IAttacker owner);
        public void Unequip();
        public void Attack(IDamageable target,AttackInfo attackInfo);
        public IWeaponStats Stats { get; }
        public T GetWeaponComponent<T>() where T : IWeaponComponent;
    }
}