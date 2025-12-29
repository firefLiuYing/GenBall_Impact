using System.Collections.Generic;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public static class BattleController 
    {
        // public static BattleController Instance => SingletonManager.GetSingleton<BattleController>();
        public static AttackResult Attack(IAttackable target, AttackInfo attackInfo)
        {
            var attackResult = target.OnAttacked(attackInfo);
            ReferencePool.Release(attackInfo);
            return attackResult;
        }

        // public static void AddBuff(IBuffable buffable, IBuff buff)
        // {
        //     buffable.AddBuff(buff);
        // }
    }


    public struct AttackResult
    {
        public bool Hit;
        public int Damage;

        public static AttackResult Create(int damage, bool hit = true)
        {
            var result = new AttackResult
            {
                Hit = hit,
                Damage = damage
            };
            return result;
        }
    }
}