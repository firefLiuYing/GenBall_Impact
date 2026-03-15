using GenBall.Utils.EntityCreator;

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
        private const string Path = "Assets/AssetBundles/Common/Bullet/";

        public static void Register(this BulletId bulletId)
        {
            var bulletName=bulletId.ToString();
            GameEntry.GetModule<EntityCreator<BulletState>>().AddPrefab<BulletState>(bulletName,Path+bulletName+".prefab");
        }

        public static BulletState Create(this BulletId bulletId)
        {
            return GameEntry.GetModule<EntityCreator<BulletState>>().CreateEntity<BulletState>(bulletId.ToString());
        }
    }
}