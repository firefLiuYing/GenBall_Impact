using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public class PlayerDashState : PlayerStateBase
    {
        private Fsm<Player> _fsm;
        // private Vector3 _dashDirection;

        // private float _prepareTime; // 前摇
        private float _invincibleTime=0.2f;  // 无敌帧
        private float _endingTime=0.1f;  // 后摇
        private float _speed=20f;   // 冲刺速度

        private float _dashCooldownTime = 4f;   // 冲刺冷却时间
        
        private Vector3 _direction;   // 冲刺方向

        private Variable<Vector2> _viewInput;
        private Variable<Quaternion> _viewRotation;
        private Variable<Vector3> _velocity;
        private Variable<bool> _onGround;
        protected internal override void OnEnter(Fsm<Player> fsm)
        {
            Debug.Log("进入冲刺态");
            _fsm = fsm;
            // _dashDirection = fsm.GetData<Variable<Vector2>>("MoveInput").Value;
            _viewInput = fsm.GetData<Variable<Vector2>>("ViewInput");
            _viewRotation = fsm.GetData<Variable<Quaternion>>("ViewRotation");
            _velocity = fsm.GetData<Variable<Vector3>>("Velocity");
            _onGround = fsm.GetData<Variable<bool>>("OnGround");
            InitArgs();
            _fsm.Owner.Countdown.Start("Dash");
        }

        protected internal override void OnExit(Fsm<Player> fsm, bool isShutdown = false)
        {
            Debug.Log("离开冲刺态");
        }

        protected internal override void OnUpdate(Fsm<Player> fsm, float elapsedTime, float realElapseTime)
        {
            ChangeView();
            if (_fsm.CurrentStateTime <= _invincibleTime)
            {
                ChangeVelocity();
            }
            else if(_fsm.CurrentStateTime >_invincibleTime+ _endingTime)
            {
                if (_onGround.Value)
                {
                    _fsm.ChangeState<PlayerMoveState>();
                }
                else
                {
                    _fsm.ChangeState<PlayerJumpState>();
                }
            }
        }

        public override void OnAttacked(AttackInfo attackInfo)
        {
            
        }
        

        // 提前计算一下冲刺方向免得重复计算
        private void InitArgs()
        {
            // 把输入从local转换到world
            // var forward=new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            // // 因为forward已经归一化了，所以fx=sin,fz=cos
            // _direction=new Vector3(_dashDirection.x*forward.z+_dashDirection.y*forward.x,0,-_dashDirection.x*forward.x+_dashDirection.y*forward.z).normalized;
            // _velocity.PostValue(_speed*direction);
            Vector3 dashDirection=Camera.main.transform.forward;
            dashDirection.y = 0;
            _direction=dashDirection.normalized;
        }
        private void ChangeVelocity()
        {
            _velocity.PostValue(_speed*_direction);
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
    }
}