namespace Yueyn.Main
{
    public interface IComponent
    {
        public void OnRegister();
        public void OnUnregister();
        public void Update(float elapsedSeconds,float realElapseSeconds);
        public void Shutdown();
    }
}