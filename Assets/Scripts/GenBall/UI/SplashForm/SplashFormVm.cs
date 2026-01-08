using GenBall.Procedure.Execute;
using Yueyn.Base.Variable;

namespace GenBall.UI
{
    public class SplashFormVm : VmBase
    {
        public readonly Variable<float> SplashProcess;

        public SplashFormVm()
        {
            SplashProcess=Variable<float>.Create();
            AddDispose(SplashProcess);
            
            RegisterEvents();
        }

        public override void Clear()
        {
            base.Clear();
            UnRegisterEvents();
        }


        private void RegisterEvents()
        {
            SplashController.Instance.SplashProcess.Observe(SplashProcess.PostValue);
        }

        private void UnRegisterEvents()
        {
            SplashController.Instance.SplashProcess.Unobserve(SplashProcess.PostValue);
        }
    }
}