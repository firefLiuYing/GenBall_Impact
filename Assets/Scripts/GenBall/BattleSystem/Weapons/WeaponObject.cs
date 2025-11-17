using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.ObjectPool;

namespace GenBall.BattleSystem.Weapons
{
    public class WeaponObject:ObjectBase
    {
        public override void OnSpawn()
        {
            if(Target is not GameObject go) return;
            go.SetActive(false);
        }

        public override void OnDespawn()
        {
            if(Target is not GameObject go) return;
            go.SetActive(false);
        }

        public static WeaponObject Create(string name, object target)
        {
            var weaponObject = ReferencePool.Acquire<WeaponObject>();
            weaponObject.Initialize(name, target);
            return weaponObject;
        }
        public override void Release(bool isShutdown)
        {
            
        }
    }
}