using GenBall.Player;
using Yueyn.Base.Variable;

namespace GenBall.UI
{
    public class MainHudVm : VmBase
    {
        public readonly Variable<int> Health;
        public readonly Variable<int> KillPoints;
        public MainHudVm()
        {
            Health = Variable<int>.Create();
            KillPoints = Variable<int>.Create();
            AddDispose(Health);
            AddDispose(KillPoints);
        }

        public void Init()
        {
            
        }

        public override void Clear()
        {
            base.Clear();
            
        }
    }
}