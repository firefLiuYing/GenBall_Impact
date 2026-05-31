using GenBall.BattleSystem.Command;
using GenBall.Event;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Handles WheelCommand: manages time-slow during wheel, fires UI events.
    /// </summary>
    public class WheelExecutor : IWheel
    {
        private object _timeScaleHandle;
        private ITimeScaleSystem _timeScaleSystem;
        private bool _isWheeling;

        public bool IsWheeling => _isWheeling;

        public void Execute(WheelCommand cmd)
        {
            _timeScaleSystem ??= SystemRepository.Instance.GetSystem<ITimeScaleSystem>();

            switch (cmd.Action)
            {
                case WheelAction.Open:
                    _isWheeling = true;
                    _timeScaleHandle = _timeScaleSystem?.Request(this, 0.2f, priority: 10);
                    CEventRouter.Instance.FireNow((int)GlobalEventId.WheelOpened);
                    break;

                case WheelAction.Confirm:
                    _isWheeling = false;
                    _timeScaleSystem?.ReleaseRequest(_timeScaleHandle);
                    _timeScaleHandle = null;
                    CEventRouter.Instance.FireNow((int)GlobalEventId.WheelConfirmed);
                    break;

                case WheelAction.Cancel:
                    _isWheeling = false;
                    _timeScaleSystem?.ReleaseRequest(_timeScaleHandle);
                    _timeScaleHandle = null;
                    CEventRouter.Instance.FireNow((int)GlobalEventId.WheelCancelled);
                    break;
            }
        }
    }
}
