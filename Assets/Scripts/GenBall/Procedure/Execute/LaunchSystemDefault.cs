using GenBall.Event;
using GenBall.Framework.Config;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class LaunchSystemDefault : ILaunchSystem, IFrameUpdate
    {
        private IConfigProvider _configProvider;
        private IGameManagerSystem _gameManager;
        private SimpleFsm<ILaunchSystem> _fsm;

        private RunningMode _runningMode;
        private string _startSceneName;
        private bool _devMode;

        /// <summary>
        /// Context set by StartGameWithContext before transitioning to LoadSceneState.
        /// </summary>
        internal GameStartContext PendingGameStartContext { get; private set; }

        public RunningMode Mode => _runningMode;
        public string StartSceneName => _startSceneName;

        public SystemScope FrameUpdateScope => SystemScope.Framework;

        public void Init()
        {
            _configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            var config = _configProvider.GetConfig<AppSettingsConfig>();
            _startSceneName = config.startSceneName;
            _runningMode = config.runningMode;
            _devMode = config.devMode;

            _gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            _gameManager.Mode = _runningMode;

            _fsm = new SimpleFsm<ILaunchSystem>(this,
                new StartupLoadingState(),
                new StartFormState(),
                new LoadSceneState()
            );

            _fsm.Start<StartupLoadingState>();
        }

        public void UnInit()
        {
            _fsm?.Shutdown();
        }

        public void FrameUpdate(float deltaTime)
        {
            if (_fsm == null || !_fsm.IsRunning)
                return;

            _fsm.Update(deltaTime);

            // StartupLoading 阶段最小展示时长，之后推进到主菜单（DevMode 下跳过等待）
            float startupLoadingMinTime = _devMode ? 0f : 1.5f;
            if (_fsm.CurrentStateType == typeof(StartupLoadingState) && _fsm.CurrentStateTime > startupLoadingMinTime)
            {
                CEventRouter.Instance.FireNow((int)GlobalEventId.StartupLoadingComplete);
                _fsm.ChangeState<StartFormState>();
            }
        }

        public void SkipStartupLoading()
        {
            if (_fsm != null && _fsm.CurrentStateType == typeof(StartupLoadingState))
            {
                CEventRouter.Instance.FireNow((int)GlobalEventId.StartupLoadingComplete);
                _fsm.ChangeState<StartFormState>();
            }
        }

        /// <summary>
        /// Called by StartFormLogic (via UI → code) after IGameStartSystem has prepared the context.
        /// Transitions the FSM to LoadSceneState with the given context.
        /// </summary>
        public void StartGameWithContext(GameStartContext context)
        {
            if (context == null)
            {
                Debug.LogError("[LaunchSystem] Cannot start game: context is null.");
                return;
            }

            PendingGameStartContext = context;
            _fsm.ChangeState<LoadSceneState>();
        }
    }
}
