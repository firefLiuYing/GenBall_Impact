using System.Collections.Generic;
using GenBall.BattleSystem.Weapons;
using GenBall.Enemy;
using GenBall.Event;
using GenBall.Event.Generated;
using GenBall.Player;
using GenBall.UI;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Event;

namespace GenBall.BattleSystem.Accessory
{
    public class AccessoryController : ISingleton
    {
        public static AccessoryController Instance => SingletonManager.GetSingleton<AccessoryController>();

        private readonly Dictionary<int, LevelConfig> _levelConfigMap = new()
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
        
        private int _level;

        public int Level
        {
            get => _level;
            private set
            {
                _level = value;
                GameEntry.Event.FireWeaponLevel(_level);
            }
        }
        
        private int _killPoints;

        public int KillPoints
        {
            get => _killPoints;
            set
            {
                _killPoints = value;
                GameEntry.Event.FirePlayerKillPoints(_killPoints);
            }
        }
        
        private int _unlockedLevel;

        public int UnlockedLevel
        {
            get => _unlockedLevel;
            set
            {
                _unlockedLevel = value;
                GameEntry.Event.FireWeaponUnlockLevel(_unlockedLevel);
            }
        }

        public void Init()
        {
            RegisterEvents();
            Level = 0;
            KillPoints = 0;
        }

        private void RegisterEvents()
        {
            GameEntry.Event.SubscribePlayerKillPoints(OnKillPointsChange);
            GameEntry.Event.SubscribeEnemyDeath(OnEnemyDeath);
        }
        private void OnKillPointsChange(int killPoints)=>UnlockedLevel = KillPointsToLevel(killPoints);

        public void Upgrade()
        {
            if(_unlockedLevel<=Level) return;
            ApplyUpgrade();
        }
        private void ApplyUpgrade()
        {
            if (Level > 0)
            {
                _levelConfigMap[Level].UnApply();
            }

            Level++;
            _levelConfigMap[Level].Apply();
            Debug.Log($"Level:{Level},UnLockedLevel:{_unlockedLevel}");
        }

        private void OnEnemyDeath(DeathInfo deathInfo)
        {
            KillPoints += deathInfo.KillPoints;
        }
        private int KillPointsToLevel(int killPoints)
        {
            var level = killPoints / 10;
            return Mathf.Min(level,4);
        }
    }
}