using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Event;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public class PlayerMoveState : PlayerStateBase
    {
        private float _speed;
        private float _verticalSensitivity;
        private float _horizontalSensitivity;
        private void InitConfigs()
        {
            _speed = _fsm.Owner.playerConfigSo.speed;
            _verticalSensitivity = _fsm.Owner.playerConfigSo.verticalSensitivity;
            _horizontalSensitivity=_fsm.Owner.playerConfigSo.horizontalSensitivity;
        }
        
        private Fsm<Player> _fsm;
        private Variable<Vector2> _moveInput;
        private Variable<Vector2> _viewInput;
        private Variable<Vector3> _velocity;
        private Variable<Quaternion> _viewRotation;
        private Variable<ButtonState> _jumpInput;
        protected internal override void OnEnter(Fsm<Player> fsm)
        {
            _fsm = fsm;
            InitConfigs();
            _moveInput = fsm.GetData<Variable<Vector2>>("MoveInput");
            _viewInput = fsm.GetData<Variable<Vector2>>("ViewInput");
            _velocity = fsm.GetData<Variable<Vector3>>("Velocity");
            _viewRotation=fsm.GetData<Variable<Quaternion>>("ViewRotation");
            _jumpInput = fsm.GetData<Variable<ButtonState>>("JumpInput");
            _viewRotation.SetValue(Camera.main.transform.rotation);
            
            RegisterEvents();
        }

        protected internal override void OnUpdate(Fsm<Player> fsm, float elapsedTime, float realElapseTime)
        {
            ChangeView();
            ChangeVelocity();
        }

        protected internal override void OnExit(Fsm<Player> fsm, bool isShutdown = false)
        {
            UnregisterEvents();   
        }
        
        public override void OnAttacked(AttackInfo attackInfo)
        {
            Debug.Log("Player: 我挨打了，我还没写挨打");
        }

        
        private void RegisterEvents()
        {
            _fsm.GetData<Variable<bool>>("OnGround").Observe(OnGroundChange);
            GameEntry.GetModule<EventManager>().Subscribe(InputEventArgs<ButtonState>.GetHashCode("JumpInput"),JumpHandler);
            _fsm.GetData<Variable<ButtonState>>("DashInput").Observe(OnDashInputChange);
        }

        private void UnregisterEvents()
        {
            _fsm.GetData<Variable<bool>>("OnGround").Unobserve(OnGroundChange);
            GameEntry.GetModule<EventManager>().Unsubscribe(InputEventArgs<ButtonState>.GetHashCode("JumpInput"),JumpHandler);
            _fsm.GetData<Variable<ButtonState>>("DashInput").Unobserve(OnDashInputChange);
        }

        private void JumpHandler(object sender, GameEventArgs e)
        {
            if(e is not InputEventArgs<ButtonState> args) return;
            if(args.Args!=ButtonState.Down) return;
            _jumpInput.SetValue(ButtonState.Down);
            _fsm.ChangeState<PlayerJumpState>();
        }
        
        private void OnGroundChange(bool onGround)
        {
            if(onGround) return;
            _jumpInput.SetValue(false);
            _fsm.ChangeState<PlayerJumpState>();
        }

        private void OnDashInputChange(ButtonState dashInput)
        {
            if(dashInput != ButtonState.Down) return;
            if(!_fsm.Owner.Countdown.HasCountdownCompleted("Dash")) return;
            _fsm.ChangeState<PlayerDashState>();
        }
        
        private void ChangeView()
        {
            var rotationEulerAngles = _viewRotation.Value.eulerAngles;
            // unity 沟槽欧拉角小巧思处理
            if (rotationEulerAngles.x is > 180 and < 360)
            {
                rotationEulerAngles.x -= 360;
            }
            rotationEulerAngles.y += _viewInput.Value.x * _horizontalSensitivity;
            rotationEulerAngles.x += -_viewInput.Value.y * _verticalSensitivity;
            rotationEulerAngles.x=Mathf.Clamp(rotationEulerAngles.x, -80, 80f);
            _viewRotation.PostValue(Quaternion.Euler(rotationEulerAngles));
        }

        private void ChangeVelocity()
        {
            // 把输入从local转换到world
            var forward=new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            // 因为forward已经归一化了，所以fx=sin,fz=cos
            var direction=new Vector3(_moveInput.Value.x*forward.z+_moveInput.Value.y*forward.x,0,-_moveInput.Value.x*forward.x+_moveInput.Value.y*forward.z).normalized;
            _velocity.PostValue(_speed*direction);
        }
    }
}