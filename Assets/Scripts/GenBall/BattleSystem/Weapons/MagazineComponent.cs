using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class MagazineComponent : WeaponComponentBase
    {
        [SerializeField] private int baseCapacity;
        public void SetBaseCapacity(int capacity) => baseCapacity=capacity;
        public readonly IntStat Capacity = new();
        public int AmmunitionCount { get;private set; }

        public void Reload()
        {
            AmmunitionCount = Capacity.CurrentValue;
        }

        public bool CanFire(int amount = 1) =>AmmunitionCount>=amount;
        public void Fire(int amount=1)
        {
            if(CanFire()) AmmunitionCount -= amount;
        }

        protected override void OnEquip()
        {
            Capacity.SetBaseValue(baseCapacity);
            Reload();
        }
    }
}