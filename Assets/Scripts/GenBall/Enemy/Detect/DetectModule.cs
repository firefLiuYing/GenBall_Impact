using System;
using JetBrains.Annotations;

namespace GenBall.Enemy.Detect
{
    public abstract class DetectModule : Module
    {
        public abstract void Search([NotNull] Action<Player.Player> findCallback);
        public abstract bool InReversoRange();
        public abstract bool InAttackRange();
        public abstract float GetTargetDistance();
    }
}