using GenBall.BattleSystem.Buff;
using UnityEngine;

namespace GenBall.BattleSystem
{
    public static class DeathEvents
    {
        public class DeathBeforeDieBuffsEvent
        {
            public DeathInfo DeathInfo;
            public IBuffContainer VictimBuffContainer;
        }

        public class DeathConfirmedEvent
        {
            public DeathInfo DeathInfo;
            public GameObject Victim;
        }

        public class DeathAfterKillBuffsEvent
        {
            public DeathInfo DeathInfo;
            public IBuffContainer KillerBuffContainer;
        }
    }
}
