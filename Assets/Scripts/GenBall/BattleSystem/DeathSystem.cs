using System.Collections.Generic;
using GenBall.BattleSystem.Buff;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public class DeathSystem : ISingleton
    {
        public static DeathSystem Instance => SingletonManager.GetSingleton<DeathSystem>();

        public void ApplyDeath(DeathInfo deathInfo)
        {
            var victim = deathInfo.Victim.GetComponent<IHealth>();
            if (victim == null)
            {
                Debug.LogError($"死者：{deathInfo.Victim}没有IHealth组件");
                ReferencePool.Release(deathInfo);
                return;
            }

            if (deathInfo.Victim.TryGetComponent<IBuffContainer>(out var victimBuffContainer))
            {
                // 触发实际死亡前触发的Buff
                var beforeDieBuffs = victimBuffContainer.GetBuffs<ITriggerBeforeDie>();
                foreach (var beforeDieBuff in beforeDieBuffs)
                {
                    beforeDieBuff.TriggerBeforeDie(deathInfo);
                }
                beforeDieBuffs.Clear();
            }

            if (deathInfo.Cancelled)
            {
                // 本次死亡被取消了
                ReferencePool.Release(deathInfo);
                return;
            }
            
            // 死亡判定成功
            
            if (victimBuffContainer != null)
            {
                // 触发死者死后触发的Buff
                var afterDieBuffs = victimBuffContainer.GetBuffs<ITriggerAfterDie>();
                foreach (var afterDieBuff in afterDieBuffs)
                {
                    afterDieBuff.TriggerAfterDie(deathInfo);
                }
                afterDieBuffs.Clear();
            }

            if (deathInfo.Killer?.TryGetComponent<IBuffContainer>(out var killerBuffContainer) ?? false)
            {
                // 触发击杀者身上击杀后触发的Buff
                var afterKillerBuffs = killerBuffContainer.GetBuffs<ITriggerAfterKill>();
                foreach (var afterKillerBuff in afterKillerBuffs)
                {
                    afterKillerBuff.TriggerAfterKill(deathInfo);
                }
                afterKillerBuffs.Clear();
            }
            
            // 实际死亡
            victim.Die(deathInfo);
            // 回收DeathInfo
            ReferencePool.Release(deathInfo);
        }
    }

    public class DeathInfo : IReference
    {
        /// <summary>
        /// 死者，不可为null
        /// </summary>
        public GameObject Victim;

        /// <summary>
        /// 击杀者，可以为null
        /// </summary>
        public GameObject Killer;

        public List<string> Tags;
        /// <summary>
        /// 被取消标识位，如果在实际触发死亡前该标识为为true，就取消死亡
        /// </summary>
        public bool Cancelled = false;

        public static DeathInfo Create(GameObject victim, List<string> tags, GameObject killer=null)
        {
            var info=ReferencePool.Acquire<DeathInfo>();
            info.Victim = victim;
            info.Killer = killer;
            info.Tags = tags;
            info.Cancelled = false;
            return info;
        }
        public void Clear()
        {
            Victim = null;
            Killer = null;
            Tags.Clear();
            Tags = null;
            Cancelled = false;
        }
    }

    public static class DeathTag
    {
        public const string HealthEmpty = "HealthEmpty";
    }
}