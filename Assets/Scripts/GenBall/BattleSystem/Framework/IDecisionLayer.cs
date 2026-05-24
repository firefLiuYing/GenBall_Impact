namespace GenBall.BattleSystem.Framework
{
    public interface IDecisionLayer
    {
        CommandDispatcherComponent Dispatcher { get; set; }
        void MakeDecision(float deltaTime);
    }
}
