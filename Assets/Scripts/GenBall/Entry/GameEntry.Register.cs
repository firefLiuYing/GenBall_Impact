using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons;
using GenBall.Enemy;
using GenBall.Map;
using GenBall.UI;
using GenBall.Utils.EntityCreator;
using GenBall.Player;
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
            RegisterEnemys();
            RegisterUIs();
        }
        private void RegisterModules()
        {
            Entry.Register(new EventManager());
            Entry.Register(new FsmManager());
            Entry.Register(new ObjectPoolManager());
            Entry.Register(new ResourceManager());
            Entry.Register(new EntityCreator<IBullet>());
            Entry.Register(new EntityCreator<IWeapon>());
            Entry.Register(new EntityCreator<IEnemy>());
            Entry.Register(new EntityCreator<IUserInterface>());
            Entry.Register(new EntityCreator<Player.Player>());
            Entry.Register(new EntityCreator<IMapBlock>());
            
            Entry.Register(GetComponentInChildren<UIManager>());
            Entry.Register(GetComponentInChildren<PlayerManager>());
            Entry.Register(GetComponentInChildren<MapModule>());
            
        }
    }
}