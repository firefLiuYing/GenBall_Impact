using System.Collections.Generic;
using GenBall.Event;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public class PauseManager : IPauseSystem
    {
        private readonly Stack<bool> _stack = new();

        public bool IsLogicPaused => _stack.Count > 0;
        public bool IsPhysicsPaused { get; private set; }
        public int StackDepth => _stack.Count;

        public void Init() { }
        public void UnInit() { }

        public void PushPause(bool pausePhysics)
        {
            _stack.Push(pausePhysics);
            ApplyState();
            FirePauseChanged();
        }

        public void PopPause()
        {
            if (_stack.Count == 0)
                return;

            _stack.Pop();
            ApplyState();
            FirePauseChanged();
        }

        private void ApplyState()
        {
            IsPhysicsPaused = _stack.Count > 0 && _stack.Peek();
            SystemUpdaterManager.Instance.SetPause(IsLogicPaused);
        }

        private static void FirePauseChanged()
        {
            CEventRouter.Instance.Fire((int)GlobalEventId.PauseChanged);
        }
    }
}
