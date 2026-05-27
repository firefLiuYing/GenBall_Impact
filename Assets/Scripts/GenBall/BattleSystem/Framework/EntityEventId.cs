namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// 实体内部事件 ID，所有 BattleEntity 共用。
    /// 通过 EventDispatcherComponent 以 FireNow 方式投递。
    /// </summary>
    public enum EntityEventId
    {
        /// <summary>StatComponent: SetBase / AddModifier / RemoveModifier</summary>
        StatChanged = 20001,

        /// <summary>DamageReceiverComponent: TakeDamage / Heal</summary>
        HealthChanged = 20002,
    }
}
