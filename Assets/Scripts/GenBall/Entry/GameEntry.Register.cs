using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons;
using GenBall.Enemy;
using GenBall.Utils.EntityCreator;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main.Entry;
using Yueyn.ObjectPool;
using Yueyn.Resource;

namespace GenBall
{
    public partial class GameEntry
    {
        private void RegisterEntityPrefabs()
        {
            RegisterBullets();
            RegisterWeapons();
        }
        private void RegisterModules()
        {
            Entry.Register(new EventManager());
            Entry.Register(new FsmManager());
            Entry.Register(new ObjectPoolManager());
            Entry.Register(new ResourceManager());
            // Entry.Register(new WeaponCreator());
            // Entry.Register(new BulletCreator());
            // Entry.Register(new EnemyCreator());
            Entry.Register(new EntityCreator<IBullet>());
            Entry.Register(new EntityCreator<IWeapon>());
        }
    }
}