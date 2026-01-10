using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons;
using GenBall.Enemy;
using GenBall.Map;
using GenBall.UI;
using GenBall.Utils.EntityCreator;
using GenBall.Player;
using GenBall.Procedure;
using GenBall.Procedure.Execute;
using GenBall.Procedure.Game;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;
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
            foreach (var com in GetComponentsInChildren<IComponent>())
            {
                _entry.Register(com);
            }
            _entry.Register(new EntityCreator<IBullet>());
            _entry.Register(new EntityCreator<IWeapon>());
            _entry.Register(new EntityCreator<IEnemy>());
            _entry.Register(new EntityCreator<IUserInterface>());
            _entry.Register(new EntityCreator<Player.Player>());
            _entry.Register(new EntityCreator<IMapBlock>());
        }

        public static EventManager Event => GetModule<EventManager>();
        public static UIManager UI => GetModule<UIManager>();
        public static SaveComponent Save => GetModule<SaveComponent>();
        public static PlayerManager Player => GetModule<PlayerManager>();
        public static MapModule Map => GetModule<MapModule>();
        public static ExecuteComponent Execute => GetModule<ExecuteComponent>();
        public static SceneModule Scene => GetModule<SceneModule>();
        public static FsmManager Fsm => GetModule<FsmManager>();
    }
}