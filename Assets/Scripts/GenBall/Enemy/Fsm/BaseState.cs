using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm
{
    public abstract class BaseState : FsmState<EnemyEntity>
    {
        protected Fsm<EnemyEntity> Fsm;
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            Fsm = fsm;
        }
        protected void ChangeState<TState>() where TState : BaseState => Fsm.ChangeState<TState>();
        protected TModule GetModule<TModule>() where TModule : Module => Fsm.Owner.GetModule<TModule>();
        protected TData GetData<TData>(string name) where TData:Variable =>Fsm.GetData<TData>(name);
    }
}