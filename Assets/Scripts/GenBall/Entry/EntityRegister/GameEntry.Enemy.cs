using GenBall.Enemy;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall
{
    public partial class GameEntry
    {
        private void RegisterEnemys()
        {
            var enemyCreator = GetModule<EntityCreator<IEnemy>>();
            enemyCreator.AddPrefab<DefaultEnemy>("Assets/AssetBundles/TemporaryAssets/Enemy/DefaultEnemy/Prefab/DefaultEnemy.prefab");
            enemyCreator.AddPrefab<EnemyBase>("NormalOrbis","Assets/AssetBundles/Common/Orbis/NormalOrbis/Prefab/NormalOrbis.prefab");
        }
    }
}