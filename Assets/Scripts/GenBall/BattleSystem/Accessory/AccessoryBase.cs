namespace GenBall.BattleSystem.Accessory
{
    public abstract class AccessoryBase : IAccessory
    {
        public IEffectable Owner { get;private set; }
        public void Apply(IEffectable target)
        {
            Owner = target;
            OnApply();
        }
        protected virtual void OnApply(){}

        public void Unapply()
        {
            OnUnapply();
            Owner = null;
        }
        protected virtual void OnUnapply(){}

        public abstract int Load { get; }
        public abstract string Name { get; }
    }
}