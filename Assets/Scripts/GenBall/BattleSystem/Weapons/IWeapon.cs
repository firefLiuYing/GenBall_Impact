using GenBall.Player;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public interface IWeapon:IEntity
    {
        public IInteractable Owner { get; }
        public void Trigger(ButtonState triggerState);
        public void OnEquip(IInteractable owner);
        public void OnUnequip();
    }
}