using GenBall.BattleSystem.Buff;
using UnityEngine;

namespace GenBall.BattleSystem
{
    public static class DamageEvents
    {
        public class DamageBeforeCauseBuffsEvent
        {
            public DamageInfo DamageInfo;
            public IBuffContainer AttackerBuffContainer;
        }

        public class DamageBeforeTakeBuffsEvent
        {
            public DamageInfo DamageInfo;
            public IBuffContainer DefenderBuffContainer;
        }

        public class DamageCompleteEvent
        {
            public DamageInfo DamageInfo;
            public GameObject Attacker;
            public GameObject Defender;
        }
    }
}
