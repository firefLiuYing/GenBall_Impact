using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public struct DefaultAttackToken : IInteractToken
    {
        public AttackArgs Args { get;private set; }
        public static DefaultAttackToken Create(IInteractable source,AttackArgs args)
        {
            var token = new DefaultAttackToken
            {
                Source = source,
                Args = args
            };
            return token;
        }

        public IInteractable Source { get; private set; }
    }
}