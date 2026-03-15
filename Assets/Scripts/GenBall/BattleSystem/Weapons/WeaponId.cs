using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public enum WeaponId
    {
        /// <summary>
        /// 开局默认的小手枪
        /// </summary>
        Pistol,
    }
    
    public static class WeaponRegister
    {
        public static void Register()
        {
            WeaponId.Pistol.Register();
        }
    }

    public static class WeaponIdExtension
    {
        private const string Path = "Assets/AssetBundles/Common/Weapon/";

        public static void Register(this WeaponId bulletId)
        {
            var bulletName=bulletId.ToString();
            GameEntry.GetModule<EntityCreator<WeaponState>>().AddPrefab<WeaponState>(bulletName,Path+bulletName+".prefab");
        }

        public static WeaponState Create(this WeaponId bulletId)
        {
            return GameEntry.GetModule<EntityCreator<WeaponState>>().CreateEntity<WeaponState>(bulletId.ToString());
        }
    }
}