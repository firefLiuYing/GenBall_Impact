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
    
}