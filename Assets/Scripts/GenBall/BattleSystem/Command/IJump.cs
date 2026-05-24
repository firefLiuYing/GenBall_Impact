namespace GenBall.BattleSystem.Command
{
    public interface IJump
    {
        void Jump(JumpCommand command);
        bool IsJumping { get; }
    }
}
