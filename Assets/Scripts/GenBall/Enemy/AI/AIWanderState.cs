using GenBall.BattleSystem.Command;
using GenBall.Enemy.Controller;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Enemy.AI
{
    public class AIWanderState : EnemyAIStateBase
    {
        private Vector3 _wanderDirection;
        private float _directionTimer;
        private float _directionChangeInterval = 3f;

        protected internal override void OnEnter(Fsm<EnemyAIController> fsm)
        {
            base.OnEnter(fsm);
            if (StateConfig.duration > 0)
                _directionChangeInterval = StateConfig.duration;
            PickRandomDirection();
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyAIController> fsm, float fixeDeltaTime)
        {
            if (Detect.HasTarget && Detect.InDetectRange)
            {
                ChangeState<AIChaseState>();
                return;
            }

            _directionTimer += fixeDeltaTime;
            if (_directionTimer >= _directionChangeInterval)
            {
                PickRandomDirection();
            }

            IssueCommand(new MoveCommand(_wanderDirection * StateConfig.moveSpeed));
            IssueCommand(new FaceDirectionCommand(_wanderDirection));
        }

        private void PickRandomDirection()
        {
            var angle = Random.Range(0f, 360f);
            _wanderDirection = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
            _directionTimer = 0f;
        }
    }
}
