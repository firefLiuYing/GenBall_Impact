using GenBall.BattleSystem.Generated;
using UnityEngine;

namespace GenBall.BattleSystem.Accessory
{
    public class FullArmorAddDamageAccessory : AccessoryBase
    {
        /// <summary>
        /// todo gzp 暂时随手填的
        /// </summary>
        public override int Load => 2;
        public override string Name => "满甲增伤配件";
        private readonly FullArmorAddDamageBuff _buff = new();
        protected override void OnApply()
        {
            // Debug.Log("安装配件");
            Owner.AddEffect(_buff);
        }

        protected override void OnUnapply()
        {
            Owner.RemoveEffect(_buff);
        }

        private class FullArmorAddDamageBuff : BuffBase
        {
            public override string Name => "满甲增伤Buff";

            protected override void OnApply()
            {
                // Debug.Log("注册Buff效果");
                RegisterEvents();
            }

            protected override void OnUnapply()
            {
                UnRegisterEvents();
            }

            private void BeforeAttackJustify(IAttackable target, AttackInfo info)
            {
                // Debug.Log("增伤判定");
                info.DamageStat.AddModifier(new IntMultiplyModifier(){MultiplyValue = 0.5f});
            }
            private void RegisterEvents()
            {
                Owner.SubscribeCombatBeforeAttackJustify(BeforeAttackJustify);
            }

            private void UnRegisterEvents()
            {
                Owner.UnsubscribeCombatBeforeAttackJustify(BeforeAttackJustify);
            }
        }
    }
}