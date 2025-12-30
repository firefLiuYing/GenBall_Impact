using System.Collections.Generic;
using GenBall.BattleSystem.Weapons;
using GenBall.Event;
using GenBall.Player;
using Yueyn.Event;

namespace GenBall.BattleSystem.Accessory
{
    public class AccessoryInfo
    {
        private int _level;

        public int Level
        {
            get=>_level;
            set
            {
                var e=ValueChangeEventArgs<int>.Create("AccessoryInfo.Level",value);
                GameEntry.GetModule<EventManager>().Fire(this,e);
                _level = value;
            }
        }

        public readonly Dictionary<int, LevelConfig> LevelConfigMap = new()
        {
            [1]=new LevelConfig
            {
                Level=1,
                BaseModule = new BaseModule()
                {
                    WeaponType = typeof(DefaultWeapon),
                },
            },
            [2]=new LevelConfig
            {
                Level=2,
                BaseModule = new BaseModule()
                {
                    WeaponType = typeof(DefaultWeapon),
                },
            },
            [3]=new LevelConfig
            {
                Level=3,
                BaseModule = new BaseModule()
                {
                    WeaponType = typeof(DefaultWeapon),
                },
            },
            [4]=new LevelConfig
            {
                Level=4,
                BaseModule = new BaseModule()
                {
                    WeaponType = typeof(DefaultWeapon),
                },
            }
        };
        
    }
}