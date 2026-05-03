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
                Debug.LogError($"���ߣ�{deathInfo.Victim}û��IHealth���");
                ReferencePool.Release(deathInfo);
                return;
            }

            if (deathInfo.Victim.TryGetComponent<IBuffContainer>(out var victimBuffContainer))
            {
                // ����ʵ������ǰ������Buff
                victimBuffContainer.GetBuffs<ITriggerBeforeDie>(out var beforeDieBuffs);
                foreach (var beforeDieBuff in beforeDieBuffs)
                {
                    beforeDieBuff.TriggerBeforeDie(deathInfo);
                }
                beforeDieBuffs.ReleaseBuffList();
            }

            if (deathInfo.Cancelled)
            {
                // ����������ȡ����
                ReferencePool.Release(deathInfo);
                return;
            }
            
            // �����ж��ɹ�
            
            if (victimBuffContainer != null)
            {
                // �����������󴥷���Buff
                victimBuffContainer.GetBuffs<ITriggerAfterDie>(out var afterDieBuffs);
                foreach (var afterDieBuff in afterDieBuffs)
                {
                    afterDieBuff.TriggerAfterDie(deathInfo);
                }
                afterDieBuffs.ReleaseBuffList();
            }

            if (deathInfo.Killer?.TryGetComponent<IBuffContainer>(out var killerBuffContainer) ?? false)
            {
                // ������ɱ�����ϻ�ɱ�󴥷���Buff
                killerBuffContainer.GetBuffs<ITriggerAfterKill>(out var afterKillerBuffs);
                foreach (var afterKillerBuff in afterKillerBuffs)
                {
                    afterKillerBuff.TriggerAfterKill(deathInfo);
                }
                afterKillerBuffs.ReleaseBuffList();
            }
            
            // ʵ������
            victim.Die(deathInfo);
            // ����DeathInfo
            ReferencePool.Release(deathInfo);
        }
    }

    public class DeathInfo : IReference
    {
        /// <summary>
        /// ���ߣ�����Ϊnull
        /// </summary>
        public GameObject Victim;

        /// <summary>
        /// ��ɱ�ߣ�����Ϊnull
        /// </summary>
        public GameObject Killer;

        public List<string> Tags;
        /// <summary>
        /// ��ȡ����ʶλ�������ʵ�ʴ�������ǰ�ñ�ʶΪΪtrue����ȡ������
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