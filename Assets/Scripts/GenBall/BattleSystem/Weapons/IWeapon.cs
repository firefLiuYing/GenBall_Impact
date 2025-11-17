using GenBall.Player;

namespace GenBall.BattleSystem.Weapons
{
    public interface IWeapon
    {
        public IAttacker Owner { get; }
        public void Trigger(ButtonState triggerState);
        public void OnEquip(IAttacker owner);
        public void OnUnequip();
    }
}