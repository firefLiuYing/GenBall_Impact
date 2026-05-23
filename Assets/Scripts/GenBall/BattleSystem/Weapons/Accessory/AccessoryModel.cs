using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GenBall.BattleSystem.Buff;

namespace GenBall.BattleSystem.Weapons.Accessory
{
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct AccessoryModel
    {
        public AccessoryId Id;
        /// <summary>
        /// ��Ϸ������չʾ���������
        /// </summary>
        public string displayName;
        /// <summary>
        /// ����ĸ���
        /// </summary>
        public int loadCost;
        /// <summary>
        /// ���Ϊ�������ӵ�Buff��Ϣ
        /// </summary>
        public List<AccessoryAddBuffInfo>  addBuffs;
    }

    [Serializable,StructLayout(LayoutKind.Auto)]
    public struct AccessoryAddBuffInfo
    {
        public string buffId;
        public int stackCount;
    }
}