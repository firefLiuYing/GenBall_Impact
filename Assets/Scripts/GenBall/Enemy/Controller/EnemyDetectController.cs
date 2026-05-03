using GenBall.BattleSystem.Character;
using UnityEngine;

namespace GenBall.Enemy.Controller
{
    public class EnemyDetectController : CharacterControllerBase
    {
        [SerializeField] private float detectRange;
        [SerializeField] private float hateRange;
        [SerializeField] private float attackRange;
        [SerializeField] private LayerMask targetLayer;

        public CharacterState CurrentTarget { get; private set; }
        public float TargetDistance { get; private set; }
        public bool HasTarget => CurrentTarget != null && !CurrentTarget.IsDead;
        public bool InDetectRange => HasTarget && TargetDistance <= detectRange;
        public bool InHateRange => HasTarget && TargetDistance <= hateRange;
        public bool InAttackRange => HasTarget && TargetDistance <= attackRange;

        public Vector3 DirectionToTarget => HasTarget
            ? (CurrentTarget.transform.position - transform.position).normalized
            : Vector3.zero;

        public override void Initialize(CharacterState characterState) { }

        public override void Tick(float deltaTime)
        {
            if (HasTarget)
            {
                TargetDistance = Vector3.Distance(transform.position, CurrentTarget.transform.position);
                if (CurrentTarget.IsDead) CurrentTarget = null;
            }
            else
            {
                SearchForTarget();
            }
        }

        private void SearchForTarget()
        {
            var colliders = Physics.OverlapSphere(transform.position, detectRange, targetLayer);
            foreach (var col in colliders)
            {
                var target = col.GetComponentInParent<CharacterState>();
                if (target != null && !target.IsDead)
                {
                    CurrentTarget = target;
                    TargetDistance = Vector3.Distance(transform.position, target.transform.position);
                    return;
                }
            }
        }

        public void ClearTarget()
        {
            CurrentTarget = null;
            TargetDistance = float.MaxValue;
        }
    }
}
