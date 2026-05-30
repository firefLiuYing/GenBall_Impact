using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Framework;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.BattleSystem
{
    public class DamageSystemDefault : IDamageSystem
    {
        private const int DamageBeforeCauseBuffs = BuffEventIds.DamageBeforeCauseBuffs;
        private const int DamageBeforeTakeBuffs = BuffEventIds.DamageBeforeTakeBuffs;
        private const int DamageComplete = BuffEventIds.DamageComplete;

        public void Init() { }
        public void UnInit() { }

        public void ApplyDamage(DamageInfo damageInfo)
        {
            if (damageInfo == null || damageInfo.Defender == null)
            {
                if (damageInfo != null) ReferencePool.Release(damageInfo);
                return;
            }

            // 1. Find IDamageable on defender — try BattleEntity components first (pure C#), then MB
            var defender = damageInfo.Defender;
            var battleEntity = defender.GetComponentInParent<BattleEntity>();
            var fromMb = defender.GetComponentInChildren<IDamageable>() ?? defender.GetComponentInParent<IDamageable>();
            var attackable = battleEntity?.Get<IDamageable>() ?? fromMb;
            if (attackable == null)
            {
                ReferencePool.Release(damageInfo);
                return;
            }
            Debug.Log($"[Damage] {defender.name} takes {damageInfo.Damage.GetValue()} dmg from {(damageInfo.Attacker ? damageInfo.Attacker.name : "?")}");

            // 2. Get IBuffContainer references
            var attackerBuffContainer = damageInfo.Attacker?.GetComponent<IBuffContainer>();
            var defenderBuffContainer = defender.GetComponent<IBuffContainer>();

            // 3. Fire events (buff triggers handled by BuffTickSystem subscribers)
            var router = CEventRouter.Instance;

            if (attackerBuffContainer != null)
                router.FireNow(DamageBeforeCauseBuffs, new DamageEvents.DamageBeforeCauseBuffsEvent
                    { DamageInfo = damageInfo, AttackerBuffContainer = attackerBuffContainer });

            if (defenderBuffContainer != null)
                router.FireNow(DamageBeforeTakeBuffs, new DamageEvents.DamageBeforeTakeBuffsEvent
                    { DamageInfo = damageInfo, DefenderBuffContainer = defenderBuffContainer });

            // 4. Apply damage
            attackable.TakeDamage(damageInfo);

            // 5. Complete event (post-damage buff triggers handled here)
            router.FireNow(DamageComplete, new DamageEvents.DamageCompleteEvent
                { DamageInfo = damageInfo, Attacker = damageInfo.Attacker, Defender = defender });

            // 6. Cleanup
            ReferencePool.Release(damageInfo);
        }
    }
}
