using System.Collections.Generic;
using GenBall.Event.Generated;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Weapons.Accessory
{
    public class EvolutionSystem:MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        public int MaxEvolutionLevel { get; private set; } = 4;

        public int CurrentEvolutionLevel
        {
            get=>_currentEvolutionLevel;
            set
            {
                _currentEvolutionLevel = value;
                GameEntry.Event.FireWeaponLevel(_currentEvolutionLevel);
            }
        }
        private int _currentEvolutionLevel;
        public int KillPoints{
            get=>_killPoints;
            private set
            {
                _killPoints = value;
                GameEntry.Event.FirePlayerKillPoints(_killPoints);
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
            _config = EvolutionConfigProvider.GetOrCreateConfig();
            KillPoints = 0;
            CurrentEvolutionLevel = 0;
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }

    public class EquipInfo
    {
        public WeaponId WeaponId;
        public List<AccessoryId> Accessories;
    }
}