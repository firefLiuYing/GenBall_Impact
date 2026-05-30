using GenBall.BattleSystem.Command;
using GenBall.Enemy.AI;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.BattleSystem.Framework.AI
{
    public class AIDecisionIdleState : EnemyDecisionStateBase
    {
        private Vector3 _wanderDirection;
        private float _directionTimer;
        private float _directionChangeInterval = 3f;

        protected internal override void OnEnter(Fsm<EnemyDecisionLayer> fsm)
        {
            base.OnEnter(fsm);
            _directionTimer = 0f;

            if (StateConfig != null)
            {
                if (StateConfig.duration > 0)
                    _directionChangeInterval = StateConfig.duration;

                if (StateConfig.idleBehavior != IdleBehavior.Stationary)
                    PickRandomDirection();
            }
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyDecisionLayer> fsm, float fixedDeltaTime)
        {
            if (Detect.HasTarget && Detect.InDetectRange)
            {
                ChangeState<AIDecisionChaseState>();
                return;
            }

            var behavior = StateConfig?.idleBehavior ?? IdleBehavior.Wander;

            switch (behavior)
            {
                case IdleBehavior.Stationary:
                    // Do nothing, just wait for target detection.
                    break;

                case IdleBehavior.Wander:
                case IdleBehavior.Patrol:
                    _directionTimer += fixedDeltaTime;
                    if (_directionTimer >= _directionChangeInterval)
                    {
                        PickRandomDirection();
                    }

                    var speed = StateConfig?.moveSpeed ?? 2f;
                    IssueCommand(new MoveCommand(_wanderDirection * speed));
                    IssueCommand(new FaceDirectionCommand(_wanderDirection));
                    break;
            }
        }

        private void PickRandomDirection()
        {
            var angle = Random.Range(0f, 360f);
            _wanderDirection = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
            _directionTimer = 0f;
        }
    }
}
