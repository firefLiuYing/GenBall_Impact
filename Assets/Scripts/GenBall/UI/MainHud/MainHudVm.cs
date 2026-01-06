using GenBall.BattleSystem.Accessory;
using GenBall.BattleSystem.Weapons;
using GenBall.Event;
using GenBall.Event.Generated;
using GenBall.Player;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Event;

namespace GenBall.UI
{
    public class MainHudVm : VmBase
    {
        public readonly Variable<int> Health;
        public readonly Variable<int> Armor;
        public readonly Variable<int> KillPoints;
        public readonly Variable<int> Level;
        public readonly Variable<int> MaxHealth;
        public readonly Variable<MagazineComponent.MagazineInfo> MagazineInfo;
        public MainHudVm()
        {
            Health = Variable<int>.Create();
            KillPoints = Variable<int>.Create();
            Armor = Variable<int>.Create();
            Level = Variable<int>.Create();
            MaxHealth = Variable<int>.Create();
            MagazineInfo = Variable<MagazineComponent.MagazineInfo>.Create();
            AddDispose(Health);
            AddDispose(Armor);
            AddDispose(KillPoints);
            AddDispose(Level);
            AddDispose(MaxHealth);
            AddDispose(MagazineInfo);
        }

        public void Init()
        {
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            GameEntry.Event.SubscribePlayerHealth(Health.PostValue);
            GameEntry.Event.SubscribePlayerArmor(Armor.PostValue);
            GameEntry.Event.SubscribePlayerKillPoints(KillPoints.PostValue);
            GameEntry.Event.SubscribePlayerMaxHealth(MaxHealth.PostValue);
            GameEntry.Event.SubscribeWeaponLevel(Level.PostValue);
            GameEntry.Event.SubscribeWeaponMagazineInfoChange(MagazineInfo.PostValue);
            GameEntry.Event.SubscribeWeaponLevel(OnLevelChanged);
            GameEntry.Event.SubscribeWeaponUnlockLevel(OnLevelChanged);
        }
        private void UnregisterEvents()
        {
            GameEntry.Event.UnsubscribePlayerHealth(Health.PostValue);
            GameEntry.Event.UnsubscribePlayerArmor(Armor.PostValue);
            GameEntry.Event.UnsubscribePlayerKillPoints(KillPoints.PostValue);
            GameEntry.Event.UnsubscribePlayerMaxHealth(MaxHealth.PostValue);
            GameEntry.Event.UnsubscribeWeaponLevel(Level.PostValue);
            GameEntry.Event.UnsubscribeWeaponMagazineInfoChange(MagazineInfo.PostValue);
            GameEntry.Event.UnsubscribeWeaponLevel(OnLevelChanged);
            GameEntry.Event.UnsubscribeWeaponUnlockLevel(OnLevelChanged);
        }

        public override void Clear()
        {
            base.Clear();
            UnregisterEvents();
        }

        private void OnLevelChanged(int level)
        {
            if (AccessoryController.Instance.Level < AccessoryController.Instance.UnlockedLevel)
            {
                UpgradeTip.Open();
            }
            else
            {
                GameEntry.GetModule<UIManager>().CloseForm<UpgradeTip>();
            }
        }
    }
}