using System.Collections.Generic;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{


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