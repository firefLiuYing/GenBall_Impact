using System.Collections.Generic;
using UnityEngine;
using Yueyn.Resource;

namespace GenBall.BattleSystem.Bullets
{
    public enum BulletId
    {
        RayBullet,
    }

    public static class BulletRegister
    {
        public static void Register()
        {
            BulletId.RayBullet.Register();
        }
    }

    public static class BulletIdExtension
    {
        private static readonly Dictionary<BulletId, string> _prefabPaths = new();
        private const string Path = "Assets/AssetBundles/Common/Bullet/";

        public static void Register(this BulletId bulletId)
        {
            var bulletName=bulletId.ToString();
            _prefabPaths[bulletId] = Path + bulletName + ".prefab";
        }

        public static BulletState Create(this BulletId bulletId)
        {
            var path = _prefabPaths[bulletId];
            var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
            var go = Object.Instantiate(prefab);
            return go.GetComponent<BulletState>();
        }
    }
}