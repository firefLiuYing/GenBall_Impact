namespace GenBall.BattleSystem.Weapons
{
    public interface IWeaponComponent
    {
        public void Equip(IWeapon owner);
        public void Unequip();
    }
}