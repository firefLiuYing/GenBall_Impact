namespace GenBall.BattleSystem.Bullets
{
    public partial class BulletCreator
    {
        private void RegisterBullets()
        {
            AddBulletPrefab<DefaultBullet>("Assets/AssetBundles/TemporaryAssets/Bullet/DefaultBullet/Prefab/DefaultBullet.prefab");
        }
    }
}