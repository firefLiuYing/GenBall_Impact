using Yueyn.Main;
using GenBall.BattleSystem.Framework;

namespace GenBall.CombatState
{
    public interface ICombatStateSystem : ISystem
    {
        bool IsInCombat { get; }
        void BindPlayer(BattleEntity player);
    }
}
