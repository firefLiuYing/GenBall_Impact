using GenBall.BattleSystem.Accessory;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Event;

namespace GenBall.Player
{
    public partial class Player
    {
        [SerializeField]internal PlayerConfigSo playerConfigSo;
        private EventManager EventManager=>GameEntry.GetModule<EventManager>();

        private void RegisterInputHandlers()
        {
            EventManager.Subscribe(InputEventArgs<Vector2>.GetHashCode("MoveInput"),OnMoveInputChange);
            EventManager.Subscribe(InputEventArgs<Vector2>.GetHashCode("ViewInput"),OnViewInputChange);
            EventManager.Subscribe(InputEventArgs<ButtonState>.GetHashCode("DashInput"),OnDashInputChange);
            EventManager.Subscribe(InputEventArgs<ButtonState>.GetHashCode("JumpInput"),OnJumpInputChange);
            EventManager.Subscribe(InputEventArgs<ButtonState>.GetHashCode("FireInput"),OnFireInputChange);
            EventManager.Subscribe(InputEventArgs<ButtonState>.GetHashCode("UpgradeInput"),OnUpgradeInputChange);
        }
        private void OnMoveInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<Vector2> args) return;
            var moveInput=_fsm.GetData<Variable<Vector2>>("MoveInput");
            moveInput.PostValue(args.Args);
        }

        private void OnViewInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<Vector2> args) return;
            var viewInput=_fsm.GetData<Variable<Vector2>>("ViewInput");
            viewInput.PostValue(args.Args);
        }

        private void OnDashInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<ButtonState> args) return;
            var dashInput=_fsm.GetData<Variable<ButtonState>>("DashInput");
            dashInput.PostValue(args.Args);
        }

        private void OnJumpInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<ButtonState> args) return;
            var jumpInput=_fsm.GetData<Variable<ButtonState>>("JumpInput");
            jumpInput.PostValue(args.Args);
        }

        private void OnFireInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<ButtonState> args) return;
            PhysicsWeaponTrigger(args.Args);
        }

        private void OnUpgradeInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<ButtonState> args) return;
            if (args.Args == ButtonState.Down)
            {
                AccessoryController.Instance.Upgrade();
            }
        }
        private void OnVelocityChange(Vector3 velocity)=>_rigidbody.velocity=velocity;

        private void OnViewRotationChange(Quaternion viewRotation)=>mainCameraTransform.rotation=viewRotation;
    }
}