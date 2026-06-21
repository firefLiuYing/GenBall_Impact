using Yueyn.Event;

namespace GenBall.Event.Params
{
    [System.Serializable]
    [EventParamHint(6003)]
    public class PlayDialogueParams : EventParameterBase
    {
        public int dialogueId;

        public override void Dispatch(int eventId)
            => CEventRouter.Instance.FireNow(eventId, this);
    }
}
