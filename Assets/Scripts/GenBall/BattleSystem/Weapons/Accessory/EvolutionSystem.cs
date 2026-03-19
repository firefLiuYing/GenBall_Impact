using System.Collections.Generic;
using GenBall.Event.Generated;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Weapons.Accessory
{
    public class EvolutionSystem:MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        public int MaxEvolutionLevel { get;private set; }

        public int CurrentEvolutionLevel
        {
            get=>_currentEvolutionLevel;
            private set
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
                if(CurrentEvolutionLevel+2>_config.loadOfLevel.Count) return false;
                return KillPoints >= _config.loadOfLevel[CurrentEvolutionLevel+1];
            }
        }
        private EvolutionConfig _config;
        private readonly List<EquipInfo>  _equipInfos=new();

        public EquipInfo GetEquipInfo(int level)
        {
            if(level<1||level>_config.loadOfLevel.Count) return null;
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