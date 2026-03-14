using System;
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
    }

    public static class BuffIdToExtension
    {
        public static Type ToType(this BuffId id)
        {
            return id switch
            {
                BuffId.PlayerArmor=>typeof(ArmorBuff),
                _ => typeof(BuffObj),
            };
        }
    }
}