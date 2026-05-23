using GenBall.BattleSystem.Buff;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.BattleSystem
{
    public class DeathSystemDefault : IDeathSystem
    {
        private const int DeathBeforeDieBuffs = BuffEventIds.DeathBeforeDieBuffs;
        private const int DeathConfirmed = BuffEventIds.DeathConfirmed;
        private const int DeathAfterKillBuffs = BuffEventIds.DeathAfterKillBuffs;

        public void Init() { }
        public void UnInit() { }

        public void ApplyDeath(DeathInfo deathInfo)
        {
            if (deathInfo == null || deathInfo.Victim == null)
            {
                if (deathInfo != null) ReferencePool.Release(deathInfo);
                return;
            }

            var victim = deathInfo.Victim;
            var health = victim.GetComponentInChildren<IHealth>()
                      ?? victim.GetComponentInParent<IHealth>();
            if (health == null)
            {
                Debug.LogError($"gzp DeathSystem: No IHealth found on {victim.name}");
                ReferencePool.Release(deathInfo);
                return;
            }

            var victimBuffContainer = victim.GetComponent<IBuffContainer>();
            var killerBuffContainer = deathInfo.Killer?.GetComponent<IBuffContainer>();
            var router = CEventRouter.Instance;

            // Before die buffs
            if (victimBuffContainer != null)
                router.FireNow(DeathBeforeDieBuffs, new DeathEvents.DeathBeforeDieBuffsEvent
                    { DeathInfo = deathInfo, VictimBuffContainer = victimBuffContainer });

            // Check cancellation
            if (deathInfo.Cancelled)
            {
                ReferencePool.Release(deathInfo);
                return;
            }

            // After die buffs (victim)
            router.FireNow(DeathConfirmed, new DeathEvents.DeathConfirmedEvent
                { DeathInfo = deathInfo, Victim = victim });

            // After kill buffs (killer)
            if (killerBuffContainer != null)
                router.FireNow(DeathAfterKillBuffs, new DeathEvents.DeathAfterKillBuffsEvent
                    { DeathInfo = deathInfo, KillerBuffContainer = killerBuffContainer });

            // Execute death
            health.Die(deathInfo);

            // Cleanup
            ReferencePool.Release(deathInfo);
        }
    }
}
