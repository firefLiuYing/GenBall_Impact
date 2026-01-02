namespace GenBall.BattleSystem.Weapons
{
    public interface IWeaponComponent
    {
        public IWeapon Owner { get; }
        public void Equip(IWeapon owner);
        public void Unequip();
    }
}