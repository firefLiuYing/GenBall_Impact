using UnityEngine;

namespace GenBall.BattleSystem.Framework
{
    public class AttackComponent : IAttacker
    {
        private readonly BattleEntity _entity;
        private readonly IDamageCalculator _calculator;

        public AttackComponent(BattleEntity entity, IDamageCalculator calculator)
        {
            _entity = entity;
            _calculator = calculator;
        }

        /// <summary>
        /// Calculate raw damage using the configured strategy.
        /// </summary>
        public int CalculateDamage(StatComponent weaponStats = null, StatComponent bulletStats = null)
        {
            var context = new DamageContext
            {
                Attacker = _entity,
                AttackerStats = _entity.Get<StatComponent>(),
                WeaponStats = weaponStats,
                BulletStats = bulletStats,
                Direction = Vector3.zero
            };
            return _calculator.Calculate(context);
        }

        /// <summary>Expose the calculator for testing/inspection.</summary>
        public IDamageCalculator Calculator => _calculator;
    }
}
