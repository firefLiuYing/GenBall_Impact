using Yueyn.Main;

namespace Yueyn.Timer
{
    public class TimerManager : IComponent
    {
        public int Priority => 100;
        public void Init()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            Timer.Update(realElapseSeconds);
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}