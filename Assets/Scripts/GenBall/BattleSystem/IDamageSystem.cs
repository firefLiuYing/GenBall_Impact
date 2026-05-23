using Yueyn.Main;

namespace GenBall.BattleSystem
{
    public interface IDamageSystem : ISystem
    {
        void ApplyDamage(DamageInfo damageInfo);
    }
}
