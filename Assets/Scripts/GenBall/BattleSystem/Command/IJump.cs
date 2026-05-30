namespace GenBall.BattleSystem.Command
{
    public interface IJump
    {
        void Jump(JumpCommand command);
        bool IsJumping { get; }

        /// <summary>
        /// Force-cancel the jump immediately (e.g., interrupted by dash).
        /// After calling, IsJumping returns false.
        /// </summary>
        void Cancel();
    }
}
