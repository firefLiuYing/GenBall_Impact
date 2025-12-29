using System.Collections.Generic;
using GenBall.Player;
using GenBall.UI;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Event;

namespace GenBall.Accessory
{
    public class AccessoryController : ISingleton
    {
        public static AccessoryController Instance => SingletonManager.GetSingleton<AccessoryController>();
        
        public readonly AccessoryInfo Accessory = new();

        private readonly List<AccessoryInfo> _accessoryPackage = new();
        
        private int _unlockedLevel;

        public void Init()
        {
            RegisterEvents();
            Accessory.Level = 0;
        }

        private void RegisterEvents()
        {
            GameEntry.GetModule<EventManager>().Subscribe(ValueChangeEventArgs<int>.GetId("ActorInfo.KillPoints"),HandleKillPointsChange);
            
        }
        private void HandleKillPointsChange(object sender, GameEventArgs e)       
        {
            if(e is not ValueChangeEventArgs<int> args) return;
            _unlockedLevel=KillPointsToLevel(args.Value);
            if (_unlockedLevel > Accessory.Level)
            {
                UpgradeTip.Open();
            }
        }

        public void Upgrade()
        {
            if(_unlockedLevel<=Accessory.Level) return;
            ApplyUpgrade();
        }
        private void ApplyUpgrade()
        {
            if (Accessory.Level > 0)
            {
                Accessory.LevelConfigMap[Accessory.Level].UnApply();
            }

            Accessory.Level++;
            Accessory.LevelConfigMap[Accessory.Level].Apply();
            Debug.Log($"Level:{Accessory.Level},UnLockedLevel:{_unlockedLevel}");
            if (_unlockedLevel <= Accessory.Level)
            {
                GameEntry.GetModule<UIManager>().CloseForm<UpgradeTip>();
            }
        }

        private int KillPointsToLevel(int killPoints)
        {
            var level = killPoints / 10;
            return Mathf.Min(level,4);
        }
    }
}