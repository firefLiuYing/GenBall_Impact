using System;

namespace GenBall.BattleSystem.Buff
{
    /// <summary>
    /// 
    /// </summary>
    public enum BuffId
    {
        Default,
        /// <summary>
        /// …’…À£¨≤‚ ‘”√
        /// </summary>
        TestBurn=1,
    }

    public static class BuffIdToExtension
    {
        public static Type ToType(this BuffId id)
        {
            return id switch
            {
                _ => typeof(BuffObj),
            };
        }
    }
}