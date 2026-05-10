using GenBall.Event.Generated;
using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class NormalReloadController : MonoBehaviour,IWeaponReloadController
    {
        [SerializeField] private int baseCapacity;
        private int _restBullets;

        public int RestBullets
        {
            get=>_restBullets;
            private set
            {
                _restBullets = value;
                FireMagazineInfoChange();
            }
        }

        public int Capacity=>baseCapacity;
        public bool HaveBullets=>RestBullets > 0;
        public WeaponState Weapon { get;private set; }
        public void Init(WeaponState weapon)
        {
            Weapon = weapon;
            RestBullets = Capacity;
        }

        public void CostBullet(int amount = 1)
        {
            if (RestBullets < amount) return;
            RestBullets -= amount;
        }
        public void Reload(ButtonState button)
        {
            if(button!=ButtonState.Down) return;
            HandleReload();
        }

        public void AutoReload()
        {
            HandleReload();
        }
        private void HandleReload()
        {
            RestBullets=Capacity;
        }
        
        private void FireMagazineInfoChange()
        {
            GameEntry.Event.FireNowWeaponMagazineInfoChange(new  MagazineInfo { AmmunitionCount = RestBullets , Capacity = Capacity });
        }
    }
}