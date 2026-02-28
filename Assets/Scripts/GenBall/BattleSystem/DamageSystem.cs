using System.Collections.Generic;
using System.Linq;
using GenBall.Utils.Singleton;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public class DamageSystem : ISingleton
    {
        public static DamageSystem Instance=>SingletonManager.GetSingleton<DamageSystem>();
        
    }

    public class DamageInfo:IReference
    {
        // todo gzp 管理单一段攻击的计算
        // 需要包括伤害计算，冲击力计算，给受击方挂载的buff，其中buff部分可以等到buff系统做好了再写，其中如果计算得出会造成受击单位死亡，需要走死亡结算流程
        
        public void Clear()
        {
            
        }
    }
    public class DamageValue:IReference
    {
        private readonly Dictionary<string, FloatStat> _multipleZones = new();
        private IntStat _baseDamageStat;
        private IntStat _addDamageStat;

        public DamageValue Create(int baseValue)
        {
            var damage=ReferencePool.Acquire<DamageValue>();
            _baseDamageStat=IntStat.Create(baseValue);
            _addDamageStat=IntStat.Create();
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
                multipleZone.Clear();
            }
            _multipleZones.Clear();
            _baseDamageStat.Clear();
            _baseDamageStat = null;
            _addDamageStat.Clear();
            _addDamageStat = null;
        }
    }
}