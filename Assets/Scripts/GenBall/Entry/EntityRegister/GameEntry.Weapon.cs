using GenBall.BattleSystem.Weapons;
using GenBall.Utils.EntityCreator;

namespace GenBall
{
    public partial class GameEntry
    {
        private void RegisterWeapons()
        {
            var weaponCreator = GetModule<EntityCreator<IWeapon>>();
            weaponCreator.AddPrefab<DefaultWeapon>("Assets/AssetBundles/TemporaryAssets/Weapon/DefaultWeapon/Prefab/DefaultWeapon.prefab");
        }
    }
}