using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public class AttackInfo : IReference
    {
        public IAttacker Attacker;
        public void Clear()
        {
            Attacker = null;
        }
    }
}