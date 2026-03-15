using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.Utils.Singleton;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public class DamageSystem : ISingleton
    {
        public static DamageSystem Instance=>SingletonManager.GetSingleton<DamageSystem>();

        /// <summary>
        /// 造成伤害的统一流程，会自动管理DamageInfo生命周期，任何伤害结算务必通过该流程
        /// </summary>
        /// <param name="damageInfo"></param>
        public void ApplyDamage(DamageInfo damageInfo)
        {
            var attackable = damageInfo.Defender.GetComponentInChildren<IDamageable>();
            if (attackable == null)
            {
                attackable = damageInfo.Defender.GetComponentInParent<IDamageable>();
                if (attackable == null)
                {
                    ReferencePool.Release(damageInfo);
                    return;
                }
            }

            IBuffContainer attackerBuffContainer=null;
            if (damageInfo.Attacker?.TryGetComponent(out attackerBuffContainer)??false)
            {
                // 触发攻击方身上造成伤害前触发的Buff 
                var beforeCauseDamageBuffs = attackerBuffContainer.GetBuffs<ITriggerBeforeCauseDamage>();
                foreach (var beforeCauseDamageBuff in beforeCauseDamageBuffs)
                {
                    beforeCauseDamageBuff.TriggerBeforeCauseDamage(damageInfo);
                }
                beforeCauseDamageBuffs.Clear();
            }

            if (damageInfo.Defender.TryGetComponent<IBuffContainer>(out var defenderBuffContainer))
            {
                // 触发受击方身上受到伤害前触发的Buff
                var beforeTakeDamageBuffs = defenderBuffContainer.GetBuffs<ITriggerBeforeTakeDamage>();
                foreach (var beforeTakeDamageBuff in beforeTakeDamageBuffs)
                {
                    beforeTakeDamageBuff.TriggerBeforeTakeDamage(damageInfo);
                }
                beforeTakeDamageBuffs.Clear();
            }
            
            // 实际造成伤害
            attackable.TakeDamage(damageInfo);
            Debug.Log($"造成了{damageInfo.Damage.GetValue()}点伤害");

            if (attackerBuffContainer != null)
            {
                // 触发攻击方身上造成伤害后触发的Buff
                var afterCauseDamageBuffs = attackerBuffContainer.GetBuffs<ITriggerAfterCauseDamage>();
                foreach (var afterCauseDamageBuff in afterCauseDamageBuffs)
                {
                    afterCauseDamageBuff.TriggerAfterCauseDamage(damageInfo);
                }
                afterCauseDamageBuffs.Clear();
            }

            if (defenderBuffContainer != null)
            {
                // 触发受击方身上受到伤害后触发的Buff
                var afterTakeDamageBuffs = defenderBuffContainer.GetBuffs<ITriggerAfterTakeDamage>();
                foreach (var afterTakeDamageBuff in afterTakeDamageBuffs)
                {
                    afterTakeDamageBuff.TriggerAfterTakeDamage(damageInfo);
                }
                afterTakeDamageBuffs.Clear();
            }
            
            // 流程结束，回收DamageInfo
            ReferencePool.Release(damageInfo);
        }
    }

    public class DamageInfo:IReference
    {
        /// <summary>
        /// 被攻击方，不可以为null
        /// </summary>
        public GameObject Defender;
        /// <summary>
        /// 攻击方，可以为null
        /// </summary>
        public GameObject Attacker;

        public List<string> Tags;
        /// <summary>
        /// 本次攻击所能造成的伤害
        /// </summary>
        public DamageValue Damage;
        /// <summary>
        /// 本次攻击所造成的冲击力
        /// </summary>
        public DamageValue ImpactForce;
        /// <summary>
        /// 本次攻击来自的方向
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// 本次攻击对目标所添加的Buff信息，会在攻击流程结束后再添加
        /// </summary>
        public List<AddBuffInfo> AddBuffs;

        public static DamageInfo Create([NotNull] GameObject defender,int damage,List<string> tags,int impactForce=0,GameObject attacker=null,List<AddBuffInfo> addBuffs = null)
        {
            return Create(defender, damage, tags,Vector3.one, impactForce, attacker, addBuffs);
        }
        public static DamageInfo Create([NotNull] GameObject defender,int damage,List<string> tags,Vector3 direction,int impactForce=0,GameObject attacker=null,List<AddBuffInfo> addBuffs = null)
        {
            var info=ReferencePool.Acquire<DamageInfo>();
            info.Defender = defender;
            info.Damage = DamageValue.Create(damage);
            info.ImpactForce = DamageValue.Create(impactForce);
            info.Attacker = attacker;
            info.AddBuffs = addBuffs;
            info.Tags = tags;
            info.Direction=direction;
            return info;
        }
        public void Clear()
        {
            Defender = null;
            Attacker = null;
            Tags?.Clear();
            Tags = null;
            ReferencePool.Release(Damage);
            Damage = null;
            ReferencePool.Release(ImpactForce);
            ImpactForce = null;
            Direction = Vector3.zero;
            if (AddBuffs != null)
            {
                AddBuffs.Clear();
                AddBuffs = null;
            }
        }
    }
    public class DamageValue:IReference
    {
        private readonly Dictionary<string, FloatStat> _multipleZones = new();
        private IntStat _baseDamageStat;
        private IntStat _addDamageStat;

        public static DamageValue Create(int baseValue)
        {
            var damage=ReferencePool.Acquire<DamageValue>();
            damage._baseDamageStat=IntStat.Create(baseValue);
            damage._addDamageStat=IntStat.Create();
            return damage;
        }
        public void AddDamage(int value)
        {
            _addDamageStat.AddModifier(AddModifier<int>.Create(value));
        }

        public void AddBaseDamage(int value)
        {
            _baseDamageStat.AddModifier(AddModifier<int>.Create(value));
        }

        /// <summary>
        /// 0.2f指该乘区倍率加0.2f，最后结算时如果该乘区倍率为0.2f，就是基础伤害乘1.2f
        /// </summary>
        /// <param name="zoneName"></param>
        /// <param name="value"></param>
        public void AddMultipleZone(string zoneName, float value)
        {
            if (_multipleZones.TryGetValue(zoneName, out var zone))
            {
                zone.AddModifier(AddModifier<float>.Create(value));
            }
            else
            {
                _multipleZones.Add(zoneName,FloatStat.Create(1));
                _multipleZones[zoneName].AddModifier(AddModifier<float>.Create(value));
            }
        }
        /// <summary>
        /// 伤害计算规则，基础伤害乘以各个乘区倍率，再加上加值
        /// </summary>
        /// <returns></returns>
        public int GetValue()
        {
            var value = _multipleZones.Values.Aggregate<FloatStat, float>(1, (current, stat) => current * stat.CurrentValue);
            value*=_baseDamageStat.CurrentValue;
            value+=_addDamageStat.CurrentValue;
            return (int)value;
        }
        public void Clear()
        {
            foreach (var multipleZone in _multipleZones.Values)
            {
                ReferencePool.Release(multipleZone);
            }
            _multipleZones.Clear();
            ReferencePool.Release(_baseDamageStat);
            _baseDamageStat = null;
            ReferencePool.Release(_addDamageStat);
            _addDamageStat = null;
        }
    }
}