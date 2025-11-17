using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class DefaultWeapon : MonoBehaviour,IWeapon
    {
        public IAttacker Owner { get;protected set; }
        [SerializeField] private Transform bulletSpawnPoint;
        public void Trigger(ButtonState triggerState)
        {
            Debug.Log(triggerState);
        }

        public void OnEquip(IAttacker owner)
        {
            Owner = owner;
            gameObject.SetActive(true);
        }

        public void OnUnequip()
        {
            
        }
    }
}