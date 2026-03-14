using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Character;

namespace GenBall.Player.Initializer
{
    public class PlayerArmorInitializer : CharacterInitializerBase
    {
        public override void Initialize(CharacterState characterState)
        {
            GameEntry.Buff.AddBuff(AddBuffInfo.Create(BuffId.PlayerArmor,characterState.gameObject));
        }
    }
}