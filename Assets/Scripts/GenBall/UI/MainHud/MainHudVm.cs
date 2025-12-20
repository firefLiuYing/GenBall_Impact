using GenBall.Player;
using Yueyn.Base.Variable;
using Yueyn.Event;

namespace GenBall.UI
{
    public class MainHudVm : VmBase
    {
        public readonly Variable<int> Health;
        public readonly Variable<int> Armor;
        public readonly Variable<int> KillPoints;
        public MainHudVm()
        {
            Health = Variable<int>.Create();
            KillPoints = Variable<int>.Create();
            Armor = Variable<int>.Create();
            AddDispose(Health);
            AddDispose(Armor);
            AddDispose(KillPoints);
        }

        public void Init()
        {
            RegisterEvents();
            
            Health.PostValue(PlayerController.Instance.Actor.Health);
            Armor.PostValue(PlayerController.Instance.Actor.Armor);
            KillPoints.PostValue(PlayerController.Instance.Actor.KillPoints);
        }

        private void RegisterEvents()
        {
            GameEntry.GetModule<EventManager>().Subscribe(ValueChangeEventArgs<int>.GetId("ActorInfo.Health"),OnHealthChanged);
            GameEntry.GetModule<EventManager>().Subscribe(ValueChangeEventArgs<int>.GetId("ActorInfo.Armor"),OnArmorChanged);
            GameEntry.GetModule<EventManager>().Subscribe(ValueChangeEventArgs<int>.GetId("ActorInfo.KillPoints"),OnKillPointsChanged);
        }
        private void UnregisterEvents()
        {
            GameEntry.GetModule<EventManager>().Unsubscribe(ValueChangeEventArgs<int>.GetId("ActorInfo.Health"),OnHealthChanged);
            GameEntry.GetModule<EventManager>().Unsubscribe(ValueChangeEventArgs<int>.GetId("ActorInfo.Armor"),OnArmorChanged);
            GameEntry.GetModule<EventManager>().Unsubscribe(ValueChangeEventArgs<int>.GetId("ActorInfo.KillPoints"),OnKillPointsChanged);
        }

        private void OnHealthChanged(object sender, GameEventArgs e)
        {
            if(e is not ValueChangeEventArgs<int> args) return;
            Health.PostValue(args.Value);
        }

        private void OnArmorChanged(object sender, GameEventArgs e)
        {
            if(e is not ValueChangeEventArgs<int> args) return;
            Armor.PostValue(args.Value);
        }

        private void OnKillPointsChanged(object sender, GameEventArgs e)
        {
            if(e is not ValueChangeEventArgs<int> args) return;
            KillPoints.PostValue(args.Value);
        }

        public override void Clear()
        {
            base.Clear();
            UnregisterEvents();
        }
    }
}