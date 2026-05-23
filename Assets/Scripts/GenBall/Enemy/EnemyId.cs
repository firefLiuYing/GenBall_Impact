using System.Collections.Generic;
using GenBall.BattleSystem.Character;
using UnityEngine;
using Yueyn.Resource;

namespace GenBall.Enemy
{
    public enum EnemyId
    {
        Default=0,
        TestOrbis=1,
    }

    public static class EnemyRegister
    {
        public static void Register()
        {
            EnemyId.TestOrbis.Register();
        }
    }
    public static class EnemyIdExtension
    {
        private static readonly Dictionary<EnemyId, string> _prefabPaths = new();
        private const string Path = "Assets/AssetBundles/Common/Orbis/NormalOrbis/Prefab/";

        public static void Register(this EnemyId enemyId)
        {
            var enemyName=enemyId.ToString();
            _prefabPaths[enemyId] = Path + enemyName + ".prefab";
        }

        public static CharacterState Create(this EnemyId enemyId, Vector3? position = null, Quaternion? rotation = null)
        {
            var path = _prefabPaths[enemyId];
            var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
            var go = Object.Instantiate(prefab, position ?? Vector3.zero, rotation ?? Quaternion.identity);
            return go.GetComponent<CharacterState>();
        }
    }
}