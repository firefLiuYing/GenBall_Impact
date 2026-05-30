using GenBall.BattleSystem.Timeline;
using GenBall.Map;
using GenBall.UI;
using GenBall.Procedure;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;
using Yueyn.Main.Entry;

namespace GenBall
{
    public partial class GameEntry
    {
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
        // ExecuteComponent / SceneModule migrated to new framework ISystems
        // (ILaunchSystem / ISceneLoadSystem)
        public static FsmManager Fsm => GetModule<FsmManager>();
        public static TimelineSystem Timeline => GetModule<TimelineSystem>();
    }
}
