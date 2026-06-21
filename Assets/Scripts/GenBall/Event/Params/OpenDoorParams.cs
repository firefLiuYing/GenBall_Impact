using Yueyn.Event;

namespace GenBall.Event.Params
{
    [System.Serializable]
    [EventParamHint(6002)]
    public class OpenDoorParams : EventParameterBase
    {
        public int doorObjectId;
        public float openSpeed = 1f;

        public override void Dispatch(int eventId)
            => CEventRouter.Instance.FireNow(eventId, this);
    }
}
