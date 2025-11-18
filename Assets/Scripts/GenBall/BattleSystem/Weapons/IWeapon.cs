using GenBall.Player;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public interface IWeapon:IEntity
    {
        public IAttacker Owner { get; }
        public void Trigger(ButtonState triggerState);
        public void OnEquip(IAttacker owner,Transform parent);
        public void OnUnequip();
    }
}