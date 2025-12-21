using System;
using GenBall.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Yueyn.Base.ReferencePool;
using Yueyn.Event;

namespace GenBall.Player
{
    public class InputController : MonoBehaviour
    {
        private EventManager _eventManager;

        private void Awake()
        {
            _eventManager = GameEntry.GetModule<EventManager>();
        }

        public void MoveInput(InputAction.CallbackContext context)
        {
            var eventArgs=ReferencePool.Acquire<InputEventArgs<Vector2>>();
            eventArgs.Name = "MoveInput";
            eventArgs.Args=context.ReadValue<Vector2>();
            _eventManager.Fire(this, eventArgs);
        }

        public void ViewInput(InputAction.CallbackContext context)
        {
            var eventArgs=ReferencePool.Acquire<InputEventArgs<Vector2>>();
            eventArgs.Name = "ViewInput";
            eventArgs.Args=context.ReadValue<Vector2>();
            _eventManager.Fire(this, eventArgs);
        }

        public void FireInput(InputAction.CallbackContext context)
        {
            var eventArgs=ReferencePool.Acquire<InputEventArgs<ButtonState>>();
            eventArgs.Name = "FireInput";
            eventArgs.Args = context.phase switch
            {
                InputActionPhase.Started=>ButtonState.Down,
                InputActionPhase.Canceled=>ButtonState.Up,
                InputActionPhase.Performed=>ButtonState.Hold,
                _=>ButtonState.None
            };
            _eventManager.Fire(this, eventArgs);
        }

        public void JumpInput(InputAction.CallbackContext context)
        {
            var eventArgs=ReferencePool.Acquire<InputEventArgs<ButtonState>>();
            eventArgs.Name = "JumpInput";
            eventArgs.Args = context.phase switch
            {
                InputActionPhase.Started=>ButtonState.Down,
                InputActionPhase.Canceled=>ButtonState.Up,
                InputActionPhase.Performed=>ButtonState.Hold,
                _=>ButtonState.None
            };
            _eventManager.Fire(this, eventArgs);
        }

        public void DashInput(InputAction.CallbackContext context)
        {
            var eventArgs=ReferencePool.Acquire<InputEventArgs<ButtonState>>();
            eventArgs.Name = "DashInput";
            eventArgs.Args = context.phase switch
            {
                InputActionPhase.Started=>ButtonState.Down,
                InputActionPhase.Canceled=>ButtonState.Up,
                InputActionPhase.Performed=>ButtonState.Hold,
                _=>ButtonState.None
            };
            _eventManager.Fire(this, eventArgs);
        }
        
        
        public void UpgradeInput(InputAction.CallbackContext context)
        {
            var eventArgs=ReferencePool.Acquire<InputEventArgs<ButtonState>>();
            eventArgs.Name = "UpgradeInput";
            eventArgs.Args = context.phase switch
            {
                InputActionPhase.Started=>ButtonState.Down,
                InputActionPhase.Canceled=>ButtonState.Up,
                InputActionPhase.Performed=>ButtonState.Hold,
                _=>ButtonState.None
            };
            _eventManager.Fire(this, eventArgs);
        }

        private bool _accessoryFormOpened = false;
        public void AccessoryInput(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                if (_accessoryFormOpened)
                {
                    GameEntry.GetModule<UIManager>().CloseForm<AccessoryForm>();
                }
                else
                {
                    GameEntry.GetModule<UIManager>().OpenForm<AccessoryForm>();
                }
                _accessoryFormOpened=!_accessoryFormOpened;
            }
        }
    }
}