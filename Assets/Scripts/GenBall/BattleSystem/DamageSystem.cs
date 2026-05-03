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
        /// ����˺���ͳһ���̣����Զ�����DamageInfo�������ڣ��κ��˺��������ͨ��������
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
                // ������������������˺�ǰ������Buff 
                attackerBuffContainer.GetBuffs<ITriggerBeforeCauseDamage>(out var beforeCauseDamageBuffs);
                foreach (var beforeCauseDamageBuff in beforeCauseDamageBuffs)
                {
                    beforeCauseDamageBuff.TriggerBeforeCauseDamage(damageInfo);
                }
                beforeCauseDamageBuffs.ReleaseBuffList();
            }

            if (damageInfo.Defender.TryGetComponent<IBuffContainer>(out var defenderBuffContainer))
            {
                // �����ܻ��������ܵ��˺�ǰ������Buff
                defenderBuffContainer.GetBuffs<ITriggerBeforeTakeDamage>(out var beforeTakeDamageBuffs);
                foreach (var beforeTakeDamageBuff in beforeTakeDamageBuffs)
                {
                    beforeTakeDamageBuff.TriggerBeforeTakeDamage(damageInfo);
                }
                beforeTakeDamageBuffs.ReleaseBuffList();
            }
            
            // ʵ������˺�
            attackable.TakeDamage(damageInfo);
            Debug.Log($"�����{damageInfo.Damage.GetValue()}���˺�");

            if (attackerBuffContainer != null)
            {
                // ������������������˺��󴥷���Buff
                attackerBuffContainer.GetBuffs<ITriggerAfterCauseDamage>(out var afterCauseDamageBuffs);
                foreach (var afterCauseDamageBuff in afterCauseDamageBuffs)
                {
                    afterCauseDamageBuff.TriggerAfterCauseDamage(damageInfo);
                }
                afterCauseDamageBuffs.ReleaseBuffList();
            }

            if (defenderBuffContainer != null)
            {
                // �����ܻ��������ܵ��˺��󴥷���Buff
                defenderBuffContainer.GetBuffs<ITriggerAfterTakeDamage>(out var afterTakeDamageBuffs);
                foreach (var afterTakeDamageBuff in afterTakeDamageBuffs)
                {
                    afterTakeDamageBuff.TriggerAfterTakeDamage(damageInfo);
                }
                afterTakeDamageBuffs.ReleaseBuffList();
            }
            
            // ���̽���������DamageInfo
            ReferencePool.Release(damageInfo);
        }
    }

    public class DamageInfo:IReference
    {
        /// <summary>
        /// ����������������Ϊnull
        /// </summary>
        public GameObject Defender;
        /// <summary>
        /// ������������Ϊnull
        /// </summary>
        public GameObject Attacker;

        public List<string> Tags;
        /// <summary>
        /// ���ι���������ɵ��˺�
        /// </summary>
        public DamageValue Damage;
        /// <summary>
        /// ���ι�������ɵĳ����
        /// </summary>
        public DamageValue ImpactForce;
        /// <summary>
        /// ���ι������Եķ���
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// ���ι�����Ŀ�������ӵ�Buff��Ϣ�����ڹ������̽�����������
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
        /// 0.2fָ�ó������ʼ�0.2f��������ʱ����ó�������Ϊ0.2f�����ǻ����˺���1.2f
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
        /// �˺�������򣬻����˺����Ը����������ʣ��ټ��ϼ�ֵ
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