using GenBall.BattleSystem.Character;
using UnityEngine;

namespace GenBall.Enemy.Initializer
{
    public class EnemyDefaultInitializer : CharacterInitializerBase
    {
        public override void Initialize(CharacterState characterState)
        {
            characterState.CanMove = true;
            characterState.CanRotate = true;
            characterState.CanAttack = true;
            characterState.gameObject.SetActive(true);
        }
    }
}
