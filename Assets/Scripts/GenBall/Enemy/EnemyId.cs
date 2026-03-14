using GenBall.BattleSystem.Character;
using GenBall.Utils.EntityCreator;
using UnityEngine;

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
        private const string Path = "Assets/AssetBundles/Common/Orbis/NormalOrbis/Prefab/";
        public static void Register(this EnemyId enemyId)
        {
            var enemyName=enemyId.ToString();
            GameEntry.CharacterCreator.AddPrefab<CharacterState>(enemyName,Path+enemyName+".prefab");
        }
    }
}