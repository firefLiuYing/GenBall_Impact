using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public class AttackInfo:IReference
    {
        public IAttacker Attacker;
        public int Damage;
        public Vector3 Direction;
        public float ImpactForce;//冲击力
        public AttackArgs ExtraArgs;//附加参数，可有可无
        public static AttackInfo Create(IAttacker attacker,int damage, Vector3 direction, float impactForce,AttackArgs  extraArgs=null)
        {
            var info = ReferencePool.Acquire<AttackInfo>();
            info.Attacker = attacker;
            info.Damage = damage;
            info.Direction = direction;
            info.ImpactForce = impactForce;
            info.ExtraArgs = extraArgs;
            return info;
        }
        public void Clear()
        {
            Attacker = null;
            Damage = 0;
            Direction = Vector3.zero;
            ImpactForce = 0;
            if (ExtraArgs != null)
            {
                ReferencePool.Release(ExtraArgs);
            }
        }
    }

    public abstract class AttackArgs:IReference
    {
        public abstract void Clear();
    }
}