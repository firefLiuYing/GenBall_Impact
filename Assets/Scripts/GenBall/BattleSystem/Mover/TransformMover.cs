using GenBall.Event;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.BattleSystem.Mover
{
    public class TransformMover : MonoBehaviour
    {
        private bool _isPaused;
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
            if (_isPaused) return;
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
            transform.position += _velocity * deltaTime;
        }

        private void OnPauseChanged()
        {
            var ps = SystemRepository.Instance.GetSystem<IPauseSystem>();
            _isPaused = ps != null && ps.IsLogicPaused;
        }

        private void OnEnable()
        {
            CEventRouter.Instance.Subscribe((int)GlobalEventId.PauseChanged, OnPauseChanged);
        }

        private void OnDisable()
        {
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.PauseChanged, OnPauseChanged);
        }
    }
}
