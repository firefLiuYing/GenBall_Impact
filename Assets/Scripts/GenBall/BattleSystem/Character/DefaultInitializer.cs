using UnityEngine;

namespace GenBall.BattleSystem.Character
{
    /// <summary>
    /// 默认初始化组件，作用只有把GameObject设为active
    /// </summary>
    public class DefaultInitializer : MonoBehaviour, ICharacterInitializer
    {
        public int Priority => int.MaxValue;

        public void Initialize(CharacterState characterState)
        {
            characterState.CanMove = true;
            characterState.CanRotate = true;
            characterState.gameObject.SetActive(true);
        }
    }
}