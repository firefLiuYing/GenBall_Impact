namespace GenBall.BattleSystem
{
    public interface IAttackable
    {
        public AttackResult OnAttacked(AttackInfo attackInfo);
    }

    public enum AttackResult
    {
        Undefined,Hit,Missed
    }
    
    public delegate AttackResult OnAttackDelegate(AttackInfo attackInfo);
    // 单词表，参考用
    // public enum AttackResult
    // {
    //     Hit,            // 命中
    //     Dodged,         // 被闪避
    //     Missed,         // 未命中（与闪避分开）
    //     Blocked,        // 被格挡
    //     Parried,        // 被招架/弹反
    //     Resisted,       // 被抵抗（魔法/属性）
    //     Immune,         // 免疫
    //     CriticalHit,    // 暴击
    //     GlancingBlow    // 擦伤/偏斜
    // }
}