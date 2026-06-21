using Yueyn.Event;

namespace GenBall.Event.Params
{
    [System.Serializable]
    [EventParamHint(6005)]
    public class UnlockSavePointParams : EventParameterBase
    {
        public int savePointId;

        public override void Dispatch(int eventId)
            => CEventRouter.Instance.FireNow(eventId, this);
    }
}
