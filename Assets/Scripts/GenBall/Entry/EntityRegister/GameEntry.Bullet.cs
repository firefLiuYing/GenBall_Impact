using GenBall.BattleSystem.Bullets;
using GenBall.Utils.EntityCreator;

namespace GenBall
{
    public partial class GameEntry
    {
        private void RegisterBullets()
        {
            var bulletCreator = GetModule<EntityCreator<IBullet>>();
            bulletCreator.AddPrefab<DefaultBullet>("Assets/AssetBundles/TemporaryAssets/Bullet/DefaultBullet/Prefab/DefaultBullet.prefab");
        }
    }
}