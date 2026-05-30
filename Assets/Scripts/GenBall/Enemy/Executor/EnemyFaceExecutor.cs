using GenBall.BattleSystem.Command;
using UnityEngine;

namespace GenBall.Enemy.Executor
{
    /// <summary>
    /// Execute layer: handles facing direction via smooth rotation.
    /// Does NOT implement IEntityLogicUpdate — rotation is computed directly in Face().
    /// </summary>
    public class EnemyFaceExecutor : IFaceDirection
    {
        private readonly Transform _transform;
        private readonly float _rotateSpeed;

        public EnemyFaceExecutor(Transform transform, float rotateSpeed)
        {
            _transform = transform;
            _rotateSpeed = rotateSpeed;
        }

        public void Face(FaceDirectionCommand command)
        {
            var dir = command.Direction;
            if (dir.sqrMagnitude < 0.001f)
                return;

            var targetRotation = Quaternion.LookRotation(dir);
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation, targetRotation,
                _rotateSpeed * Time.deltaTime);
        }
    }
}
