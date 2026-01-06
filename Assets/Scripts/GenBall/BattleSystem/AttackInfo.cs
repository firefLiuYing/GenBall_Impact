using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public class AttackInfo:IReference
    {
        public IAttacker Attacker;
        public int Damage=>DamageStat.CurrentValue;
        public Vector3 Direction;
        public float ImpactForce=>ImpactForceStat.CurrentValue;//冲击力

        public readonly IntStat DamageStat=new IntStat();
        public readonly FloatStat ImpactForceStat=new FloatStat();
        // public AttackArgs ExtraArgs;//附加参数，可有可无
        public static AttackInfo Create(IAttacker attacker,int damage, Vector3 direction, float impactForce)
        {
            var info = ReferencePool.Acquire<AttackInfo>();
            info.Attacker = attacker;
            info.DamageStat.SetBaseValue(damage);
            info.Direction = direction;
            info.ImpactForceStat.SetBaseValue(impactForce);
            // info.ExtraArgs = extraArgs;
            return info;
        }
        public void Clear()
        {
            Attacker = null;
            Direction = Vector3.zero;
            DamageStat.ResetStat();
            ImpactForceStat.ResetStat();
            // if (ExtraArgs != null)
            // {
            //     ReferencePool.Release(ExtraArgs);
            // }
        }
    }

    // public abstract class AttackArgs:IReference
    // {
    //     public abstract void Clear();
    // }
}