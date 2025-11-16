using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Event;

namespace GenBall.Player
{
    public partial class Player
    {
        [SerializeField]internal PlayerConfigSo playerConfigSo;
        private void OnVelocityChange(Vector3 velocity)=>_rigidbody.velocity=velocity;

        private void OnViewRotationChange(Quaternion viewRotation)=>Camera.main.transform.rotation=viewRotation;
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
    }
}