using Yueyn.Main;

namespace GenBall.BattleSystem
{
    public interface IDeathSystem : ISystem
    {
        void ApplyDeath(DeathInfo deathInfo);
    }
}
