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
        /// 游戏内用于展示的配件名称
        /// </summary>
        public string displayName;
        /// <summary>
        /// 配件的负载
        /// </summary>
        public int loadCost;
        /// <summary>
        /// 配件为武器添加的Buff信息
        /// </summary>
        public List<AccessoryAddBuffInfo>  addBuffs;
    }

    [Serializable,StructLayout(LayoutKind.Auto)]
    public struct AccessoryAddBuffInfo
    {
        public BuffId buffId;
        public int stackCount;
    }
}