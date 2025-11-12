using System;

namespace Yueyn.Fsm
{
    public interface IFsm
    {
        public Type OwnerType { get; }
        public string Name { get;set; }
        public string FullName { get; }
        public bool IsRunning { get; }
        public bool IsDestroyed { get; }
        public float CurrentStateTime { get; }
        public void Update(float elapsedTime, float realElapsedTime);
        public void Shutdown();
    }
}