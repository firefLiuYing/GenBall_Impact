using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public interface IPauseSystem : ISystem
    {
        bool IsLogicPaused { get; }
        bool IsPhysicsPaused { get; }

        /// <summary>
        /// Pause game logic (input, AI, decision layers).
        /// If pausePhysics is true, also freeze physics (menu/dialog).
        /// If false, only logic is paused (cutscene — physics still runs).
        /// Stack-based: PopPause restores the previous state.
        /// </summary>
        void PushPause(bool pausePhysics);
        void PopPause();
        int StackDepth { get; }
    }
}
