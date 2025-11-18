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

            // todo ²âÊÔ´úÂë£¬¼ÇµÃÉ¾³ı
            enemyCreator.CreateEntity<DefaultEnemy>(new Vector3(5,5,3),Quaternion.identity).Initialize();
        }
    }
}