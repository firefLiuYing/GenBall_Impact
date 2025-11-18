namespace Yueyn.Main
{
    public interface IComponent
    {
        public void OnRegister();
        public void OnUnregister();
        public void ComponentUpdate(float elapsedSeconds,float realElapseSeconds);
        public void ComponentFixedUpdate(float fixedDeltaTime);
        public void Shutdown();
    }
}