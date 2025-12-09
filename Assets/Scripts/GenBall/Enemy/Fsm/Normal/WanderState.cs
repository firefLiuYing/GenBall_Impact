using GenBall.Enemy.Move;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class WanderState : BaseState
    {
        private Fsm<EnemyEntity> _fsm;
        private MoveModule  _moveModule;
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            _fsm = fsm;
            _moveModule = _fsm.Owner.GetModule<MoveModule>();
            
            // todo gzp ≤‚ ‘¥˙¬Î
            _moveModule.MoveTo(new Vector3(0,0,1));    
        }
        
    }
}