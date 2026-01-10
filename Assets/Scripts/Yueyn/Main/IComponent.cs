namespace Yueyn.Main
{
    public interface IComponent
    {
        public int Priority { get; }
        public void Init();
        public void OnUnregister();
        public void ComponentUpdate(float elapsedSeconds,float realElapseSeconds);
        public void ComponentFixedUpdate(float fixedDeltaTime);
        public void Shutdown();
    }
}