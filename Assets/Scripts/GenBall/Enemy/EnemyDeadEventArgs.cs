using Yueyn.Base.ReferencePool;
using Yueyn.Event;

namespace GenBall.Enemy
{
    public class EnemyDeadEventArgs : GameEventArgs
    {
        public override int Id => Index;
        public static int Index=> typeof(EnemyDeadEventArgs).GetHashCode();
        public IEnemy Enemy;
        public int KillPoints;

        public static EnemyDeadEventArgs Create(IEnemy enemy)
        {
            var args = ReferencePool.Acquire<EnemyDeadEventArgs>();
            args.Enemy = enemy;
            return args;
        }
        public override void Clear()
        {
            Enemy=null;
            KillPoints=0;
        }
    }
}