using GenBall.BattleSystem.Command;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Player.Executor
{
    public class WeaponVisibilityExecutor : IWeaponVisibility, IEntityLogicUpdate
    {
        private GameObject _weaponGameObject;
        private float _transitionTimer;
        private bool _targetVisible;
        private bool _isTransitioning;
        private const float TransitionDuration = 0.15f;

        public WeaponVisibilityExecutor(GameObject weaponGameObject)
        {
            _weaponGameObject = weaponGameObject;
        }

        public bool IsTransitioning => _isTransitioning;

        public void SetWeapon(GameObject weaponGameObject)
        {
            _weaponGameObject = weaponGameObject;
        }

        public void Execute(WeaponVisibilityCommand cmd)
        {
            _targetVisible = cmd.Visible;
            _transitionTimer = TransitionDuration;
            _isTransitioning = true;
        }

        public void LogicUpdate(float deltaTime)
        {
            if (!_isTransitioning) return;
            _transitionTimer -= deltaTime;
            if (_transitionTimer <= 0f)
            {
                _isTransitioning = false;
                if (_weaponGameObject != null)
                    _weaponGameObject.SetActive(_targetVisible);
            }
        }
    }
}
