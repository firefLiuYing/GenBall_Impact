namespace Yueyn.Fsm
{
    public abstract class FsmState<T> where T : class
    {
        public FsmState()
        {
        }

        protected internal virtual void OnInit(Fsm<T> fsm)
        {
            
        }

        protected internal virtual void OnEnter(Fsm<T> fsm)
        {
            
        }

        protected internal virtual void OnExit(Fsm<T> fsm,bool isShutdown=false)
        {
            
        }
        protected internal virtual void OnUpdate(Fsm<T> fsm,float elapsedTime,float realElapseTime)
        {
            
        }
        protected internal virtual void OnDestroy(Fsm<T> fsm)
        {
            
        }
        
    }
}