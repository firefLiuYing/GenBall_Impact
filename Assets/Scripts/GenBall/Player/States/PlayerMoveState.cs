using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public class PlayerMoveState : PlayerStateBase
    {
        private Variable<Vector2> _moveInput;
        private Variable<Vector2> _viewInput;
        private Variable<Vector3> _velocity;
        private Variable<Quaternion> _viewRotation;
        protected internal override void OnEnter(Fsm<Player> fsm)
        {
            _moveInput = fsm.GetData<Variable<Vector2>>("MoveInput");
            _viewInput = fsm.GetData<Variable<Vector2>>("ViewInput");
            _velocity = fsm.GetData<Variable<Vector3>>("Velocity");
            _viewRotation=fsm.GetData<Variable<Quaternion>>("ViewRotation");
            _viewRotation.SetValue(Camera.main.transform.rotation);
        }

        protected internal override void OnUpdate(Fsm<Player> fsm, float elapsedTime, float realElapseTime)
        {
            ChangeView();
            ChangeVelocity();
        }
        
        public override void OnAttacked(AttackInfo attackInfo)
        {
            Debug.Log("Player: 我挨打了，我还没写挨打");
        }

        private void ChangeView()
        {
            var rotationEulerAngles = _viewRotation.Value.eulerAngles;
            // unity 沟槽欧拉角小巧思处理
            if (rotationEulerAngles.x is > 180 and < 360)
            {
                rotationEulerAngles.x -= 360;
            }
            rotationEulerAngles.y += _viewInput.Value.x * 0.1f;
            rotationEulerAngles.x += -_viewInput.Value.y * 0.1f;
            rotationEulerAngles.x=Mathf.Clamp(rotationEulerAngles.x, -80, 80f);
            _viewRotation.PostValue(Quaternion.Euler(rotationEulerAngles));
        }

        private void ChangeVelocity()
        {
            // 把输入从local转换到world
            var forward=new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            // 因为forward已经归一化了，所以fx=sin,fz=cos
            var direction=new Vector3(_moveInput.Value.x*forward.z+_moveInput.Value.y*forward.x,0,-_moveInput.Value.x*forward.x+_moveInput.Value.y*forward.z).normalized;
            _velocity.PostValue(5*direction);
        }
    }
}