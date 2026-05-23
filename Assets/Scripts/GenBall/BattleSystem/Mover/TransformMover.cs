using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Mover
{
    public class TransformMover : MonoBehaviour
    {
        private static bool IsPaused => SystemRepository.Instance.GetSystem<IPauseSystem>().IsPaused;
        private Vector3 _velocity = Vector3.zero;

        public void SetVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }
        private bool _setPosHandled = true;
        private Vector3 _targetPosition = Vector3.zero;

        public void SetPosition(Vector3 position)
        {
            _targetPosition = position;
            _setPosHandled = false;
        }
        private bool _setRotHandled = true;
        private Quaternion _targetRotation = Quaternion.identity;

        public void SetRotation(Quaternion rotation)
        {
            _targetRotation = rotation;
            _setRotHandled = false;
        }

        public void Tick(float deltaTime)
        {
            if(IsPaused) return;
            if (!_setPosHandled)
            {
                transform.position = _targetPosition;
                _setPosHandled = true;
            }

            if (!_setRotHandled)
            {
                transform.rotation = _targetRotation;
                _setRotHandled = true;
            }
            transform.position += _velocity*deltaTime;
        }
    }
}