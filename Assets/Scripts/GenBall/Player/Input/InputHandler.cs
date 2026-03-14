using System;
using GenBall.BattleSystem.Timeline;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenBall.Player.Input
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private float jumpBufferedTime = 0.01f;
        public Vector3 MoveDirection{get; private set;}
        public bool IsJumpPressed{get; private set;}
        public float JumpHoldTime{get; private set;}
        
        private Vector2 _moveInput;

        public void OnDashInputChange(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                GameEntry.Timeline.AddTimeline(new AddTimelineInfo("PlayerDash",1f,transform.parent.gameObject));
            }
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
        }
        public Action OnInteract;
        public void OnInteractInputChange(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                OnInteract?.Invoke();
            }
        }

        public Action<float> OnScrollChange; 
        public void OnScrollInputChange(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                OnScrollChange?.Invoke(context.ReadValue<Vector2>().y);
            }
        }
        private void FixedUpdate()
        {
            if (IsJumpPressed)
            {
                JumpHoldTime+=Time.fixedDeltaTime;
            }
            // 把输入从local转换到world
            var forward=new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            // 因为forward已经归一化了，所以fx=sin,fz=cos
            MoveDirection=new Vector3(_moveInput.x*forward.z+_moveInput.y*forward.x,0,-_moveInput.x*forward.x+_moveInput.y*forward.z).normalized;
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
            OnViewInputChange?.Invoke(input);
        }
    }
}