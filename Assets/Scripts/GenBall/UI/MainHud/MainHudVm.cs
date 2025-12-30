using GenBall.BattleSystem.Accessory;
using GenBall.Event;
using GenBall.Player;
using GenBall.Player.Generated;
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
        public MainHudVm()
        {
            Health = Variable<int>.Create();
            KillPoints = Variable<int>.Create();
            Armor = Variable<int>.Create();
            Level = Variable<int>.Create();
            AddDispose(Health);
            AddDispose(Armor);
            AddDispose(KillPoints);
            AddDispose(Level);
        }

        public void Init()
        {
            RegisterEvents();
            
            Health.PostValue(PlayerController.Instance.Actor.Health);
            Armor.PostValue(PlayerController.Instance.Actor.Armor);
            KillPoints.PostValue(PlayerController.Instance.Actor.KillPoints);
            Level.PostValue(AccessoryController.Instance.Accessory.Level);
        }

        private void RegisterEvents()
        {
            GameEntry.Event.SubscribePlayerHealth(Health.PostValue);
            GameEntry.Event.SubscribePlayerArmor(Armor.PostValue);
            GameEntry.Event.SubscribePlayerKillPoints(KillPoints.PostValue);
        }
        private void UnregisterEvents()
        {
            GameEntry.Event.UnsubscribePlayerHealth(Health.PostValue);
            GameEntry.Event.UnsubscribePlayerArmor(Armor.PostValue);
            GameEntry.Event.UnsubscribePlayerKillPoints(KillPoints.PostValue);
        }

        public override void Clear()
        {
            base.Clear();
            UnregisterEvents();
        }
    }
}