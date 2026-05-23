using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Timeline;
using GenBall.BattleSystem.Weapons;
using GenBall.Enemy;
using GenBall.Map;
using GenBall.UI;
using GenBall.Procedure;
using GenBall.Procedure.Execute;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;
using Yueyn.Main.Entry;

namespace GenBall
{
    public partial class GameEntry
    {
        private void RegisterEntityPrefabs()
        {
            EnemyRegister.Register();
            BulletRegister.Register();
            WeaponRegister.Register();
        }
        private void RegisterModules()
        {
            foreach (var com in GetComponentsInChildren<IComponent>())
            {
                _entry.Register(com);
            }
        }

        public static EventManager Event => GetModule<EventManager>();
        public static UIManager UI => GetModule<UIManager>();
        public static SaveComponent Save => GetModule<SaveComponent>();
        public static MapModule Map => GetModule<MapModule>();
        public static ExecuteComponent Execute => GetModule<ExecuteComponent>();
        public static SceneModule Scene => GetModule<SceneModule>();
        public static FsmManager Fsm => GetModule<FsmManager>();
        public static TimelineSystem Timeline => GetModule<TimelineSystem>();
    }
}
