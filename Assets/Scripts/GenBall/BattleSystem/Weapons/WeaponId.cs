using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Resource;
using Object = UnityEngine.Object;

namespace GenBall.BattleSystem.Weapons
{
    public enum WeaponId
    {
        Pistol,
    }

    [Obsolete]
    public static class WeaponRegister
    {
        public static void Register()
        {
            WeaponId.Pistol.Register();
        }
    }

    [Obsolete]
    public static class WeaponIdExtension
    {
        private static readonly Dictionary<WeaponId, string> _prefabPaths = new();
        private const string Path = "Assets/AssetBundles/Common/Weapon/";

        public static void Register(this WeaponId weaponId)
        {
            var weaponName=weaponId.ToString();
            _prefabPaths[weaponId] = Path + weaponName + ".prefab";
        }

        public static WeaponState Create(this WeaponId weaponId)
        {
            var path = _prefabPaths[weaponId];
            var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
            var go = Object.Instantiate(prefab);
            return go.GetComponent<WeaponState>();
        }
    }
}