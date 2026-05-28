namespace GenBall.BattleSystem.Command
{
    public interface ISwitchWeapon
    {
        bool IsSwitching { get; }
        void SwitchWeapon(SwitchWeaponCommand cmd);
    }
}
