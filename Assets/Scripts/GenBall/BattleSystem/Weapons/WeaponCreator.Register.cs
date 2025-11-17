namespace GenBall.BattleSystem.Weapons
{
    public partial class WeaponCreator
    {
        private void RegisterWeapons()
        {
            AddWeaponPrefab<DefaultWeapon>("Assets/AssetBundles/TemporaryAssets/Weapon/DefaultWeapon/Prefab/DefaultWeapon.prefab");
        }
    }
}