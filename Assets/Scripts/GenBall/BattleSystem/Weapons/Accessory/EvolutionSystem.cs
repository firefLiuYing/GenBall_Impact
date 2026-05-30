using System.Collections.Generic;
using GenBall.Event;
using GenBall.Event.Generated;
using Yueyn.Event;

namespace GenBall.BattleSystem.Weapons.Accessory
{
    public class EvolutionSystem : IEvolutionSystem
    {
        public int MaxEvolutionLevel { get; private set; } = 4;

        public int CurrentEvolutionLevel
        {
            get=>_currentEvolutionLevel;
            set
            {
                _currentEvolutionLevel = value;
                CEventRouter.Instance.Fire(GlobalEventIds.Weapon_Level, _currentEvolutionLevel);
            }
        }
        private int _currentEvolutionLevel;
        public int KillPoints{
            get=>_killPoints;
            private set
            {
                _killPoints = value;
                CEventRouter.Instance.Fire(GlobalEventIds.Player_KillPoints, _killPoints);
            }
        }

        private int _killPoints;
        public bool CanEvolve
        {
            get
            {
                if(CurrentEvolutionLevel+1>MaxEvolutionLevel) return false;
                if(CurrentEvolutionLevel+2>_config.stageConfigs.Count) return false;
                return KillPoints >= _config.stageConfigs[CurrentEvolutionLevel+1].needKillPoints;
            }
        }
        private EvolutionConfig _config;
        private readonly List<EquipInfo>  _equipInfos=new()
        {
            new EquipInfo()
            {
                WeaponId = WeaponId.Pistol,
                Accessories = new List<AccessoryId>()
                {
                    AccessoryId.BulletDamageUp,
                }
            }
        };

        private readonly EquipInfo _defaultEquipInfo=new EquipInfo{WeaponId = WeaponId.Pistol};
        public EquipInfo GetEquipInfo(int level)
        {
            if(level<1||level>_equipInfos.Count) return _defaultEquipInfo;
            return _equipInfos[level-1];
        }
        
        public void Init()
        {
            #if UNITY_EDITOR
            _config = EvolutionConfigProvider.GetOrCreateConfig();
            #else
            _config=new EvolutionConfig();
            #endif
            KillPoints = 0;
            CurrentEvolutionLevel = 0;
        }

        public void AddKillPoints(int points)
        {
            if (points <= 0) return;
            KillPoints += points;
            // Also fire the int-based event for MainHudFormLogic (new HUD)
            CEventRouter.Instance.FireNow((int)GlobalEventId.KillPointsChanged, KillPoints);
        }

        public void UnInit()
        {

        }
    }

    public class EquipInfo
    {
        public WeaponId WeaponId;
        public List<AccessoryId> Accessories;
    }
}