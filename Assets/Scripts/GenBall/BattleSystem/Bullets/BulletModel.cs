using System;
using System.Runtime.InteropServices;

namespace GenBall.BattleSystem.Bullets
{
    [System.Obsolete("Replaced by BulletConfigEntry. Will be removed in Phase E cleanup.")]
    [Serializable,StructLayout(LayoutKind.Auto)]
    public struct BulletModel
    {
        public BulletId Id;
        /// <summary>
        /// ��ײ�뾶
        /// </summary>
        public float Radius;
        /// <summary>
        /// �ӵ����й����п������ж��ٴ�������
        /// </summary>
        public int HitTimes;
        /// <summary>
        /// ������Զ������ͬһ��Ŀ�꣬���������ж�֮�����С���     
        /// </summary>
        public float SameTargetDelay;
        /// <summary>
        /// �������е���
        /// </summary>
        public bool HitFoe;
        /// <summary>
        /// ���������Ѿ�
        /// </summary>
        public bool HitAlly;
        /// <summary>
        /// �ӵ������ٶ�
        /// </summary>
        public float Speed;
        /// <summary>
        /// �ӵ������˺�
        /// </summary>
        public int Damage;
    }
}