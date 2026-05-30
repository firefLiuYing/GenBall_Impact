using System;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem.Character
{
    [Obsolete]
    public class CharacterStats:IReference
    {
        public IntStat MaxHealth;
        public static CharacterStats Create(CharacterStatsModel model)
        {
            var stats=ReferencePool.Acquire<CharacterStats>();
            stats.MaxHealth=IntStat.Create(model.BaseHealth);
            return stats;
        }
        public void Clear()
        {
            ReferencePool.Release(MaxHealth);
        }
    }

    [Obsolete]
    [Serializable]
    public struct CharacterStatsModel
    {
        [SerializeField] private int baseHealth;

        public int BaseHealth => baseHealth;
    }
}