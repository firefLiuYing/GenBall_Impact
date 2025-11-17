using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class DefaultWeapon : MonoBehaviour,IWeapon
    {
        private IAttacker _owner;
        [SerializeField] private Transform bulletSpawnPoint;
        public void Trigger(ButtonState triggerState)
        {
            Debug.Log(triggerState);
        }

        public void OnEquip(IAttacker owner)
        {
            _owner = owner;
        }

        public void OnUnequip()
        {
            
        }
    }
}