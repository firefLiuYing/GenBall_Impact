using System;
using GenBall.BattleSystem;

namespace GenBall.Enemy
{
    [Obsolete]
    public interface IEnemy : IDamageable
    {
        public void Initialize();
    }
}