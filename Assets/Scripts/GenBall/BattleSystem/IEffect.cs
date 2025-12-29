namespace GenBall.BattleSystem
{
    public interface IEffect
    {
        public IEffectable Owner { get; }
        public void Apply(IEffectable target);
        public void Unapply();
    }
}