using GenBall.AbilityWeapon;
using UnityEngine;

namespace GenBall.UI
{
    /// <summary>
    /// Form 层提供给 WheelSlotPart 的数据。
    /// Form 决定每个 slot 的内容和含义（武器/取消/空）。
    /// </summary>
    public class AbilityWheelSlotData
    {
        public AbilityWeaponId? WeaponId;
        public string DisplayName;
        public Sprite Icon;
        public bool IsCancel;
    }
}
