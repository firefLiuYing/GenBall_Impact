using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Player;
using JetBrains.Annotations;
using Yueyn.Utils;

namespace GenBall.Accessory
{
    public class LevelConfig
    {
        private readonly List<Accessory> _accessories = new();
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
        public bool SetAccessory([NotNull] List<Accessory> accessories)
        {
            if(accessories.Count > MaxAccessoryCount) return false;
            int totalLoad = accessories.Select(accessory=>accessory.Load).Sum();
            if(totalLoad > MaxLoad) return false;
            _accessories.Clear();
            _accessories.AddRange(accessories);
            return true;
        }
        public void Apply()
        {
            if (BaseModule?.WeaponType == null)
            {
                throw new Exception("gzp BaseModule is null");
            }

            if (BaseModule.WeaponName.IsNullOrEmpty())
            {
                PlayerController.Instance.Player.EquipPhysicsWeapon(BaseModule.WeaponType);
            }
            else
            {
                PlayerController.Instance.Player.EquipPhysicsWeapon(BaseModule.WeaponName,BaseModule.WeaponType);
            }
            foreach (var accessory in _accessories)
            {
                accessory.Apply();
            }
        }
        public void UnApply()
        {
            foreach (var accessory in _accessories)
            {
                accessory.UnApply();
            }
        }
    }

    public class BaseModule
    {
        public string ModuleName { get; set; }
        public string WeaponName{get;set;}
        public Type WeaponType { get; set; }
    }

    public abstract class Accessory
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int Level { get; }

        /// <summary>
        /// todo gzp 暂时先硬编码吧
        /// </summary>
        public virtual int Load => Level switch
        {
            1 => 2,
            2 => 5,
            3 => 8,
            4 => 20,
            _ => 0
        };
        public abstract void Apply();
        public abstract void UnApply();
        public abstract List<IEnhanceEffect> Effects { get; }
    }
    
}