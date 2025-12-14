using GenBall.Player;
using Yueyn.Base.Variable;

namespace GenBall.UI
{
    public class MainHudVm : VmBase
    {
        public readonly Variable<int> Health;
        public readonly Variable<int> Kills;
        public MainHudVm()
        {
            Health = Variable<int>.Create();
            Kills = Variable<int>.Create();
            AddDispose(Health);
            AddDispose(Kills);
        }

        public void Init()
        {
            PlayerController.Instance.Health.Observe(Health.PostValue);
            Health.PostValue(PlayerController.Instance.Health.Value);
            PlayerController.Instance.Kills.Observe(Kills.PostValue);
            Kills.PostValue(PlayerController.Instance.Kills.Value);
        }
    }
}