using System;
using System.Runtime.InteropServices;

namespace GenBall.BattleSystem.Bullets
{
    [Serializable,StructLayout(LayoutKind.Auto)]
    public struct BulletModel
    {
        public BulletId Id;
        /// <summary>
        /// 碰撞半径
        /// </summary>
        public float Radius;
        /// <summary>
        /// 子弹飞行过程中可以命中多少次再销毁
        /// </summary>
        public int HitTimes;
        /// <summary>
        /// 如果可以多次命中同一个目标，两次命中判定之间的最小间隔     
        /// </summary>
        public float SameTargetDelay;
        /// <summary>
        /// 可以命中敌人
        /// </summary>
        public bool HitFoe;
        /// <summary>
        /// 可以命中友军
        /// </summary>
        public bool HitAlly;
        /// <summary>
        /// 子弹飞行速度
        /// </summary>
        public float Speed;
        /// <summary>
        /// 子弹基础伤害
        /// </summary>
        public int Damage;
    }
}