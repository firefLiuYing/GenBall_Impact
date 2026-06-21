using Yueyn.Event;

namespace GenBall.Event.Params
{
    [System.Serializable]
    [EventParamHint(6004)]
    public class GrantAccessoryParams : EventParameterBase
    {
        public int accessoryId;

        public override void Dispatch(int eventId)
            => CEventRouter.Instance.FireNow(eventId, this);
    }
}
