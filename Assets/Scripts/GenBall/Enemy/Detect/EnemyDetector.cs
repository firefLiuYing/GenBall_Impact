using GenBall.BattleSystem.Framework;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Enemy.Detect
{
    /// <summary>
    /// Pure C# enemy detector. Replaces EnemyDetectController (which depends on CharacterState).
    /// Uses BattleEntity + StatComponent for alive checks instead of CharacterState.IsDead.
    /// </summary>
    public class EnemyDetector : IEntityLogicUpdate
    {
        private readonly Transform _transform;
        private readonly LayerMask _targetLayer;
        private readonly float _detectRange;
        private readonly float _hateRange;
        private readonly float _attackRange;

        public GameObject CurrentTarget { get; private set; }
        public float TargetDistance { get; private set; }
        public bool HasTarget => CurrentTarget != null;
        public bool InDetectRange => HasTarget && TargetDistance <= _detectRange;
        public bool InHateRange => HasTarget && TargetDistance <= _hateRange;
        public bool InAttackRange => HasTarget && TargetDistance <= _attackRange;

        public Vector3 DirectionToTarget => HasTarget
            ? (CurrentTarget.transform.position - _transform.position).normalized
            : Vector3.zero;

        public EnemyDetector(Transform transform, LayerMask targetLayer,
            float detectRange, float hateRange, float attackRange)
        {
            _transform = transform;
            _targetLayer = targetLayer;
            _detectRange = detectRange;
            _hateRange = hateRange;
            _attackRange = attackRange;
        }

        public void LogicUpdate(float deltaTime)
        {
            if (HasTarget)
            {
                TargetDistance = Vector3.Distance(_transform.position,
                    CurrentTarget.transform.position);

                // Check if target is dead via BattleEntity's StatComponent
                var entity = CurrentTarget.GetComponent<BattleEntity>();
                if (entity != null)
                {
                    var stats = entity.Get<StatComponent>();
                    if (stats != null && stats.GetValue("CurrentHealth") <= 0)
                    {
                        ClearTarget();
                        return;
                    }
                }
            }
            else
            {
                SearchForTarget();
            }
        }

        private void SearchForTarget()
        {
            var origin = _transform.position + Vector3.up * 0.5f; // Orbis center offset
            var colliders = Physics.OverlapSphere(origin, _detectRange, _targetLayer);

            foreach (var col in colliders)
            {
                var entity = col.GetComponentInParent<BattleEntity>();
                if (entity == null) continue;

                var stats = entity.Get<StatComponent>();
                if (stats != null && stats.GetValue("CurrentHealth") <= 0) continue;

                CurrentTarget = entity.gameObject;
                TargetDistance = Vector3.Distance(_transform.position,
                    CurrentTarget.transform.position);
                return;
            }
        }

        public void ClearTarget()
        {
            CurrentTarget = null;
            TargetDistance = float.MaxValue;
        }
    }
}
