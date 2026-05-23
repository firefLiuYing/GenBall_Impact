using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Character;
using Yueyn.Main;

namespace GenBall.Player.Initializer
{
    public class PlayerArmorInitializer : CharacterInitializerBase
    {
        public override void Initialize(CharacterState characterState)
        {
            SystemRepository.Instance.GetSystem<IBuffRegistry>().AddBuff(AddBuffInfo.Create(BuffId.PlayerArmor,characterState.gameObject));
        }
    }
}