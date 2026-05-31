using System;
using GenBall.GameCamera;
using GenBall.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Yueyn.Main;

namespace GenBall.Player.Input
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private float jumpBufferedTime = 0.01f;
        public Vector3 MoveDirection{get; private set;}
        public Vector2 ViewDelta { get; private set; }
        public bool IsJumpPressed{get; private set;}
        public float JumpHoldTime{get; private set;}
        public bool IsDashPressed { get; private set; }
        public bool IsFirePressed { get; set; }
        public bool IsReloadPressed { get; set; }
        public bool IsSwitchWeaponPressed { get; set; }

        public event Action<ButtonState> OnJump;
        public event Action<ButtonState> OnDash;
        public event Action<ButtonState> OnFire;
        public event Action<ButtonState> OnReload;
        public event Action<ButtonState> OnSwitchWeapon;
        public event Action<ButtonState> OnAbilitySecondary;
        public event Action<ButtonState> OnAbilityWheel;
        public bool IsAbilitySecondaryPressed { get; set; }
        public bool IsAbilityWheelPressed { get; set; }

        private Vector2 _moveInput;
        private ICameraSystem _cameraSystem;

        public void OnDashInputChange(InputAction.CallbackContext context)
        {
            IsDashPressed = (context.phase == InputActionPhase.Started);
            var state = context.phase switch
            {
                InputActionPhase.Started => ButtonState.Down,
                InputActionPhase.Canceled => ButtonState.Up,
                InputActionPhase.Performed => ButtonState.Hold,
                _ => ButtonState.None
            };
            OnDash?.Invoke(state);
        }
        public void OnMoveInputChange(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>().normalized;
        }

        private float _lastJumpTime=-100f;
        InputActionPhase _jumpInputActionPhase;
        public void OnJumpInputChange(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    IsJumpPressed=true;
                    _lastJumpTime=Time.time;
                    JumpHoldTime = 0f;
                    break;
                case InputActionPhase.Canceled:
                    IsJumpPressed=false;
                    break;
                default:
                    break;
            }
            _jumpInputActionPhase=context.phase;
            var state = context.phase switch
            {
                InputActionPhase.Started => ButtonState.Down,
                InputActionPhase.Canceled => ButtonState.Up,
                InputActionPhase.Performed => ButtonState.Hold,
                _ => ButtonState.None
            };
            OnJump?.Invoke(state);
        }
        public Action OnInteract;
        public void OnInteractInputChange(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                OnInteract?.Invoke();
            }
        }

        public void OnFireInputChange(InputAction.CallbackContext context)
        {
            var state = context.phase switch
            {
                InputActionPhase.Started => ButtonState.Down,
                InputActionPhase.Canceled => ButtonState.Up,
                InputActionPhase.Performed => ButtonState.Hold,
                _ => ButtonState.None
            };
            IsFirePressed = (state != ButtonState.None && state != ButtonState.Up);
            OnFire?.Invoke(state);
        }

        public void OnReloadInputChange(InputAction.CallbackContext context)
        {
            var state = context.phase switch
            {
                InputActionPhase.Started => ButtonState.Down,
                InputActionPhase.Canceled => ButtonState.Up,
                _ => ButtonState.None
            };
            IsReloadPressed = (state == ButtonState.Down);
            OnReload?.Invoke(state);
        }

        public void OnSwitchWeaponInputChange(InputAction.CallbackContext context)
        {
            var state = context.phase switch
            {
                InputActionPhase.Started => ButtonState.Down,
                InputActionPhase.Canceled => ButtonState.Up,
                _ => ButtonState.None
            };
            IsSwitchWeaponPressed = (state == ButtonState.Down);
            OnSwitchWeapon?.Invoke(state);
        }

        public void OnAbilitySecondaryInputChange(InputAction.CallbackContext context)
        {
            var state = context.phase switch
            {
                InputActionPhase.Started => ButtonState.Down,
                InputActionPhase.Canceled => ButtonState.Up,
                InputActionPhase.Performed => ButtonState.Hold,
                _ => ButtonState.None
            };
            IsAbilitySecondaryPressed = (state != ButtonState.None && state != ButtonState.Up);
            OnAbilitySecondary?.Invoke(state);
        }

        public void OnAbilityWheelInputChange(InputAction.CallbackContext context)
        {
            var state = context.phase switch
            {
                InputActionPhase.Started => ButtonState.Down,
                InputActionPhase.Canceled => ButtonState.Up,
                _ => ButtonState.None
            };
            IsAbilityWheelPressed = (state == ButtonState.Down);
            OnAbilityWheel?.Invoke(state);
        }

        public Action<float> OnScrollChange;
        public void OnScrollInputChange(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                OnScrollChange?.Invoke(context.ReadValue<Vector2>().y);
            }
        }
        private void Awake()
        {
            _cameraSystem = SystemRepository.Instance.GetSystem<ICameraSystem>();
        }

        private void FixedUpdate()
        {
            if (IsJumpPressed)
            {
                JumpHoldTime+=Time.fixedDeltaTime;
            }
            // Convert local input to world-space direction relative to camera facing
            var mainCamera = _cameraSystem?.MainCamera;
            var camForward = mainCamera != null ? mainCamera.transform.forward : Vector3.forward;
            var forward = new Vector3(camForward.x, 0, camForward.z).normalized;
            MoveDirection = new Vector3(_moveInput.x*forward.z+_moveInput.y*forward.x,0,-_moveInput.x*forward.x+_moveInput.y*forward.z).normalized;
        }

        public bool ConsumeBufferedJump()
        {
            // Debug.Log($"{Time.time - _lastJumpTime}");
            if (Time.time - _lastJumpTime <= jumpBufferedTime)
            {
                return true;
            }
            return false;
        }

        public Action<Vector2> OnViewInputChange;
        public void OnLookInputChange(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<Vector2>();
            ViewDelta = input;
            OnViewInputChange?.Invoke(input);
        }
    }
}