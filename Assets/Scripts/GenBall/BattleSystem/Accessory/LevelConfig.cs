using System;
using GenBall.Player;
using Yueyn.Utils;

namespace GenBall.BattleSystem.Accessory
{
    public class LevelConfig
    {
        // private readonly List<AccessoryBase> _accessories = new();
        public int Level;
        public BaseModule BaseModule;
        /// <summary>
        /// todo gzp  先写死了，就这样吧
        /// </summary>
        private int MaxLoad => Level switch
        {
            1 => 7,
            2 => 15,
            3 => 25,
            4 => 40,
            _ => 0
        };

        /// <summary>
        /// todo gzp 暂时先写死了
        /// </summary>
        private int MaxAccessoryCount => Level switch
        {
            1 => 2,
            2 => 3,
            3 => 3,
            4 => 4,
            _ => 0
        };
        // public bool SetAccessory([NotNull] List<AccessoryBase> accessories)
        // {
        //     if(accessories.Count > MaxAccessoryCount) return false;
        //     int totalLoad = accessories.Select(accessory=>accessory.Load).Sum();
        //     if(totalLoad > MaxLoad) return false;
        //     _accessories.Clear();
        //     _accessories.AddRange(accessories);
        //     return true;
        // }
        public void Apply()
        {
            if (BaseModule?.WeaponType == null)
            {
                throw new Exception("gzp BaseModule is null");
            }

            var newWeapon = BaseModule.WeaponName.IsNullOrEmpty() 
                ? PlayerController.Instance.Player.EquipPhysicsWeapon(BaseModule.WeaponType) 
                : PlayerController.Instance.Player.EquipPhysicsWeapon(BaseModule.WeaponName,BaseModule.WeaponType);
            // foreach (var accessory in _accessories)
            // {
            //     // accessory.Apply(newWeapon);
            // }
        }
        public void UnApply()
        {
            // foreach (var accessory in _accessories)
            // {
            //     // accessory.UnApply();
            // }
        }
    }

    public class BaseModule
    {
        public string ModuleName { get; set; }
        public string WeaponName{get;set;}
        public Type WeaponType { get; set; }
    }

    
    
}