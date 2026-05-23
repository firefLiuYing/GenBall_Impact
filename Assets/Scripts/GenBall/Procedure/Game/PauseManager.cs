using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public class PauseManager : IPauseSystem
    {
        public bool IsPaused { get; private set; } = false;

        public void Init() { }
        public void UnInit() { }

        public void SetPause(bool paused)
        {
            IsPaused = paused;
            Debug.Log($"gzp 游戏暂停状态修改：{paused}");
            if (paused)
                SystemUpdaterManager.Instance.Pause();
            else
                SystemUpdaterManager.Instance.Resume();
        }
    }
}
