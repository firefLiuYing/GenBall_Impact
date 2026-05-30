namespace GenBall.BattleSystem.Command
{
    public interface IStun
    {
        void Stun(StunCommand command);
        bool IsStunned { get; }
    }
}
