using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Weapons;
using GenBall.Procedure.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenBall.Player.Controller
{
    public class WeaponController : CharacterControllerBase
    {
        [SerializeField] private Transform weaponSpawnPoint;
        private CharacterState  _player;
        private WeaponState _currentWeapon;
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            _currentWeapon= WeaponId.Pistol.Create();
            _currentWeapon.transform.SetParent(weaponSpawnPoint,false);
            _currentWeapon.gameObject.SetActive(true);
            _currentWeapon.Init(_player);
        }

        public void OnFireInputChange(InputAction.CallbackContext context)
        {
            if((PauseManager.Instance.State&PauseState.LogicPaused)==PauseState.LogicPaused) return;
            var buttonState = context.phase switch
            {
                InputActionPhase.Started=>ButtonState.Down,
                InputActionPhase.Canceled=>ButtonState.Up,
                InputActionPhase.Performed=>ButtonState.Hold,
                _=>ButtonState.None
            };
            _currentWeapon?.Trigger(buttonState);
        }
        public override void Tick(float deltaTime)
        {
            
        }
    }
}