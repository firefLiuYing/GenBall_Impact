using System.Collections.Generic;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.BattleSystem.Buff
{
    public class BuffTickSystem : IBuffTickSystem
    {
        private IBuffRegistry _registry;
        private readonly List<BuffObj> _cachedBuffs = new();

        public SystemScope LogicUpdateScope => SystemScope.Game;

        public void Init()
        {
            _registry = SystemRepository.Instance.GetSystem<IBuffRegistry>();

            var router = CEventRouter.Instance;
            router.Subscribe<DamageEvents.DamageBeforeCauseBuffsEvent>(
                BuffEventIds.DamageBeforeCauseBuffs, OnDamageBeforeCauseBuffs);
            router.Subscribe<DamageEvents.DamageBeforeTakeBuffsEvent>(
                BuffEventIds.DamageBeforeTakeBuffs, OnDamageBeforeTakeBuffs);
            router.Subscribe<DamageEvents.DamageCompleteEvent>(
                BuffEventIds.DamageComplete, OnDamageComplete);
            router.Subscribe<DeathEvents.DeathBeforeDieBuffsEvent>(
                BuffEventIds.DeathBeforeDieBuffs, OnDeathBeforeDieBuffs);
            router.Subscribe<DeathEvents.DeathConfirmedEvent>(
                BuffEventIds.DeathConfirmed, OnDeathConfirmed);
            router.Subscribe<DeathEvents.DeathAfterKillBuffsEvent>(
                BuffEventIds.DeathAfterKillBuffs, OnDeathAfterKillBuffs);
        }

        public void UnInit()
        {
            var router = CEventRouter.Instance;
            router.Unsubscribe<DamageEvents.DamageBeforeCauseBuffsEvent>(
                BuffEventIds.DamageBeforeCauseBuffs, OnDamageBeforeCauseBuffs);
            router.Unsubscribe<DamageEvents.DamageBeforeTakeBuffsEvent>(
                BuffEventIds.DamageBeforeTakeBuffs, OnDamageBeforeTakeBuffs);
            router.Unsubscribe<DamageEvents.DamageCompleteEvent>(
                BuffEventIds.DamageComplete, OnDamageComplete);
            router.Unsubscribe<DeathEvents.DeathBeforeDieBuffsEvent>(
                BuffEventIds.DeathBeforeDieBuffs, OnDeathBeforeDieBuffs);
            router.Unsubscribe<DeathEvents.DeathConfirmedEvent>(
                BuffEventIds.DeathConfirmed, OnDeathConfirmed);
            router.Unsubscribe<DeathEvents.DeathAfterKillBuffsEvent>(
                BuffEventIds.DeathAfterKillBuffs, OnDeathAfterKillBuffs);
            _cachedBuffs.Clear();
        }

        public void LogicUpdate(float deltaTime)
        {
            _cachedBuffs.Clear();
            foreach (var buff in _registry.ActiveBuffs)
                _cachedBuffs.Add(buff);
            foreach (var buff in _cachedBuffs)
                buff.Tick(deltaTime);
        }

        // Event handlers — run IBuff trigger methods on the appropriate entity

        private void OnDamageBeforeCauseBuffs(DamageEvents.DamageBeforeCauseBuffsEvent e)
        {
            if (e.AttackerBuffContainer == null) return;
            e.AttackerBuffContainer.GetBuffs<ITriggerBeforeCauseDamage>(out var buffs);
            foreach (var buff in buffs) buff.TriggerBeforeCauseDamage(e.DamageInfo);
            buffs.ReleaseBuffList();
        }

        private void OnDamageBeforeTakeBuffs(DamageEvents.DamageBeforeTakeBuffsEvent e)
        {
            if (e.DefenderBuffContainer == null) return;
            e.DefenderBuffContainer.GetBuffs<ITriggerBeforeTakeDamage>(out var buffs);
            foreach (var buff in buffs) buff.TriggerBeforeTakeDamage(e.DamageInfo);
            buffs.ReleaseBuffList();
        }

        private void OnDamageComplete(DamageEvents.DamageCompleteEvent e)
        {
            var attackerContainer = e.Attacker?.GetComponent<IBuffContainer>();
            if (attackerContainer != null)
            {
                attackerContainer.GetBuffs<ITriggerAfterCauseDamage>(out var buffs);
                foreach (var buff in buffs) buff.TriggerAfterCauseDamage(e.DamageInfo);
                buffs.ReleaseBuffList();
            }
            var defenderContainer = e.Defender?.GetComponent<IBuffContainer>();
            if (defenderContainer != null)
            {
                defenderContainer.GetBuffs<ITriggerAfterTakeDamage>(out var buffs);
                foreach (var buff in buffs) buff.TriggerAfterTakeDamage(e.DamageInfo);
                buffs.ReleaseBuffList();
            }
        }

        private void OnDeathBeforeDieBuffs(DeathEvents.DeathBeforeDieBuffsEvent e)
        {
            if (e.VictimBuffContainer == null) return;
            e.VictimBuffContainer.GetBuffs<ITriggerBeforeDie>(out var buffs);
            foreach (var buff in buffs) buff.TriggerBeforeDie(e.DeathInfo);
            buffs.ReleaseBuffList();
        }

        private void OnDeathConfirmed(DeathEvents.DeathConfirmedEvent e)
        {
            var victimContainer = e.Victim?.GetComponent<IBuffContainer>();
            if (victimContainer != null)
            {
                victimContainer.GetBuffs<ITriggerAfterDie>(out var buffs);
                foreach (var buff in buffs) buff.TriggerAfterDie(e.DeathInfo);
                buffs.ReleaseBuffList();
            }
        }

        private void OnDeathAfterKillBuffs(DeathEvents.DeathAfterKillBuffsEvent e)
        {
            if (e.KillerBuffContainer == null) return;
            e.KillerBuffContainer.GetBuffs<ITriggerAfterKill>(out var buffs);
            foreach (var buff in buffs) buff.TriggerAfterKill(e.DeathInfo);
            buffs.ReleaseBuffList();
        }
    }
}
