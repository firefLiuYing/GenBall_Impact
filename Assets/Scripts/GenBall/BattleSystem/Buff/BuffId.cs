using System;
using GenBall.BattleSystem.Buff.Accessory;
using GenBall.BattleSystem.Buff.Player;

namespace GenBall.BattleSystem.Buff
{
    /// <summary>
    /// 
    /// </summary>
    public enum BuffId
    {
        Default,
        PlayerArmor=1,
        BulletDamageUp=2,
    }

    public static class BuffIdToExtension
    {
        public static Type ToType(this BuffId id)
        {
            return id switch
            {
                BuffId.PlayerArmor=>typeof(ArmorBuff),
                BuffId.BulletDamageUp=>typeof(BulletDamageUpBuff),
                _ => typeof(BuffObj),
            };
        }
    }
}