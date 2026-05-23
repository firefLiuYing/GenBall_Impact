using GenBall.BattleSystem.Character;
using GenBall.Event.Generated;
using UnityEngine;

namespace GenBall.Enemy.Initializer
{
    public class EnemyDeathHandler : CharacterInitializerBase
    {
        [SerializeField] private int killPoints = 10;
        private CharacterState _characterState;

        public override void Initialize(CharacterState characterState)
        {
            _characterState = characterState;
            _characterState.OnHealthChange += OnHealthChange;
        }

        private void OnHealthChange(int health)
        {
            if (health > 0 || _characterState.IsDead) return;
            OnDeath();
        }

        private void OnDeath()
        {
            GameEntry.Event.FireEnemyDeath(new DeathInfo { KillPoints = killPoints });
            Object.Destroy(_characterState.gameObject);
        }
    }
}
