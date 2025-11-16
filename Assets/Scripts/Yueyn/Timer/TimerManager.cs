using Yueyn.Main;

namespace Yueyn.Timer
{
    public class TimerManager : IComponent
    {
        public void OnRegister()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void Update(float elapsedSeconds, float realElapseSeconds)
        {
            Timer.Update(realElapseSeconds);
        }

        public void Shutdown()
        {
            
        }
    }
}