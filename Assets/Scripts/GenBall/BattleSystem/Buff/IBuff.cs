using System.Collections.Generic;

namespace GenBall.BattleSystem.Buff
{
    public interface IBuff
    {
        public int Priority { get; }
        /// <summary>
        /// 可同时存在多个的不支持叠层
        /// </summary>
        public bool CanMultiExist { get; }
        public IReadOnlyList<string> Tags { get; }
    }
    // todo gzp 实现各个回调点接口

    #region Buff回调点

    /// <summary>
    /// 死亡时，在实际死亡前触发
    /// </summary>
    public interface ITriggerBeforeDie : IBuff
    {
        public void TriggerBeforeDie(DeathInfo deathInfo);
    }

    /// <summary>
    /// 死亡时，死亡判定成功后，实际死亡方法调用前触发
    /// </summary>
    public interface ITriggerAfterDie : IBuff
    {
        public void TriggerAfterDie(DeathInfo deathInfo);
    }

    /// <summary>
    /// 击杀时，击杀判定成功后，调用实际死亡方法前触发
    /// </summary>
    public interface ITriggerAfterKill : IBuff
    {
        public void TriggerAfterKill(DeathInfo deathInfo);
    }
    /// <summary>
    /// 攻击者发起攻击时，实际造成伤害前触发
    /// </summary>
    public interface ITriggerBeforeCauseDamage : IBuff
    {
        public void TriggerBeforeCauseDamage(DamageInfo damageInfo);
    }

    /// <summary>
    /// 攻击者发起攻击时，实际造成伤害后触发
    /// </summary>
    public interface ITriggerAfterCauseDamage : IBuff
    {
        public void TriggerAfterCauseDamage(DamageInfo damageInfo);
    }

    /// <summary>
    /// 受击者受到攻击时，实际受到伤害前触发
    /// </summary>
    public interface ITriggerBeforeTakeDamage : IBuff
    {
        public void TriggerBeforeTakeDamage(DamageInfo damageInfo);
    }

    /// <summary>
    /// 受击者受到攻击时，实际受到伤害之后触发
    /// </summary>
    public interface ITriggerAfterTakeDamage : IBuff
    {
        public void TriggerAfterTakeDamage(DamageInfo damageInfo);
    }
    
    
    /// <summary>
    /// 被添加其他buff时，在实际添加上前触发
    /// </summary>
    public interface ITriggerBeforeAddBuff : IBuff
    {
        public void TriggerBeforeAddBuff(AddBuffInfo addBuffInfo);
    }

    /// <summary>
    /// 被添加其他buff时，在实际添加上之后触发
    /// </summary>
    public interface ITriggerAfterAddBuff : IBuff
    {
        public void TriggerAfterAddBuff(AddBuffInfo addBuffInfo);
    }
    
    /// <summary>
    /// 在其他Buff进行叠层时，在实际叠层前触发
    /// </summary>
    public interface ITriggerBeforeStackBuff : IBuff
    {
        public void TriggerBeforeStackBuff(AddBuffInfo addBuffInfo);
    }

    /// <summary>
    /// 在其他Buff进行叠层时，在实际叠层后触发
    /// </summary>
    public interface ITriggerAfterStackBuff : IBuff
    {
        public void TriggerAfterStackBuff(AddBuffInfo addBuffInfo);
    }

    #endregion

    public class DefaultComparerBuff : IComparer<IBuff>
    {
        public int Compare(IBuff x, IBuff y)
        {
            if(x==null&&y==null) return 0;
            if(x==null) return 1;
            if(y==null) return -1;
            return x.Priority.CompareTo(y.Priority);
        }
    }
    
}