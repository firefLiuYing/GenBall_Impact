using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Event;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public class PlayerJumpState : PlayerStateBase
    {
        private float _shortPressJumpHeight = 1f;
        private float _longPressJumpMaxHeight = 3f;
        private float _longPressMaxTime = 0.8f;
        private float _shortPressJustifyTime = 0.02f;
        private float _gravityAcceleration = 20f;
        private float _maxDropVelocity = 8f;
        
        private float _coyoteTime = 0.01f;  // 土狼时间，先假设是0.01f
        private float _jumpInputBufferTime = 0.01f; // 连续跳跃支持0.01f的预输入，在地面检测判定成功后再跳一次

        private float _speed;

        private void InitConfigs()
        {
            _speed = _fsm.Owner.playerConfigSo.speed;
            
            _shortPressJumpHeight = _fsm.Owner.playerConfigSo.shortPressJumpHeight;
            _longPressJumpMaxHeight = _fsm.Owner.playerConfigSo.longPressJumpMaxHeight;
            _longPressMaxTime = _fsm.Owner.playerConfigSo.longPressMaxTime;
            _shortPressJustifyTime = _fsm.Owner.playerConfigSo.shortPressJustifyTime;
            _gravityAcceleration = _fsm.Owner.playerConfigSo.gravityAcceleration;
            _maxDropVelocity = _fsm.Owner.playerConfigSo.maxDropVelocity;
            _coyoteTime = _fsm.Owner.playerConfigSo.coyoteTime;
            _jumpInputBufferTime = _fsm.Owner.playerConfigSo.jumpInputBufferTime;
        }

        private float _pressedAcceleration;     // 按住时的衰减速度
        private float _releasedAcceleration;
        private float _initialVelocity;         // 起跳初速度
        
        private Fsm<Player> _fsm;
        private Variable<Vector3> _velocity;
        private Variable<ButtonState> _jumpInput;
        private Variable<Vector2> _moveInput;
        private Variable<Vector2> _viewInput;
        private Variable<Quaternion> _viewRotation;
        private Variable<bool> _jumpPreInput;
        private float _lastJumpInputTime=-1f;
        private bool _jumpToThisState;
        private float _releaseJumpButtonTime;
        protected internal override void OnEnter(Fsm<Player> fsm)
        {
            // Debug.Log("进入跳跃态");
            _fsm = fsm;
            InitConfigs();
            InitArgs();
            _velocity=fsm.GetData<Variable<Vector3>>("Velocity");
            _jumpInput = fsm.GetData<Variable<ButtonState>>("JumpInput");
            _moveInput = fsm.GetData<Variable<Vector2>>("MoveInput");
            _viewInput = fsm.GetData<Variable<Vector2>>("ViewInput");
            _viewRotation = fsm.GetData<Variable<Quaternion>>("ViewRotation");
            _jumpPreInput = fsm.GetData<Variable<bool>>("JumpPreInput");
            _jumpToThisState = _jumpInput.Value == ButtonState.Down||_jumpPreInput.Value;
            _releaseJumpButtonTime = 0f;
            if (_jumpPreInput.Value&&_jumpInput.Value is ButtonState.Up or ButtonState.None)
            {
                _releaseJumpButtonTime = _shortPressJustifyTime;
            }
            // Debug.Log(_jumpToThisState);
            _jumpPreInput.SetValue(false);
            _lastJumpInputTime = -1f;
            RegisterEvents();
        }

        protected internal override void OnUpdate(Fsm<Player> fsm, float elapsedTime, float realElapseTime)
        {
            ChangeView();
            ChangeVelocity();
        }

        protected internal override void OnExit(Fsm<Player> fsm,bool isShutdown=false)
        {
            // Debug.Log("离开跳跃态");
            UnRegisterEvents();
            _lastJumpInputTime = -1f;
            _releaseJumpButtonTime = 0f;
        }

        public override void OnAttacked(AttackArgs attackArgs)
        {
            
        }

        private void InitArgs()
        {
            // 计算长按短按跳跃所需要的参数
            // 初速度
            _initialVelocity = 2 * _longPressJumpMaxHeight / _longPressMaxTime;
            // 按住时衰减速度
            _pressedAcceleration = _initialVelocity / _longPressMaxTime;
            // 短按过程中上升高度，中间变量
            float shortPressPeriodHeight = _initialVelocity * _shortPressJustifyTime -_pressedAcceleration * _shortPressJustifyTime * _shortPressJustifyTime / 2;
            // 短按松开期间剩余要上升的高度，中间变量
            float remainHeight=_shortPressJumpHeight-shortPressPeriodHeight;
            // 松开后减速时间
            float remainTime=2 * remainHeight / (_initialVelocity - _shortPressJustifyTime * _pressedAcceleration);
            _releasedAcceleration = (_initialVelocity - _shortPressJustifyTime * _pressedAcceleration)/remainTime;
        }
        private float GetVerticalVelocityStrategyDefault() => _fsm.CurrentStateTime < 0.2f ? 2 : -2;

        private float GetVerticalVelocity()
        {
            // 自由落体进入跳跃态
            if (!_jumpToThisState)
            {
                return Mathf.Max(-_maxDropVelocity, -_fsm.CurrentStateTime * _gravityAcceleration);
            }
            // 跳跃进入跳跃态
            // 在短按时间内
            if (_fsm.CurrentStateTime<_shortPressJustifyTime)
            {
                // Debug.Log("短按跳跃");
                return _initialVelocity-_pressedAcceleration*_fsm.CurrentStateTime;
            }
            // 按键松开，或者超出长按时间
            if (_releaseJumpButtonTime > 0)
            {
                // 计算速度降为0时的时间
                float dropToZeroTime = (_initialVelocity - _releaseJumpButtonTime * _pressedAcceleration) / _releasedAcceleration+_releaseJumpButtonTime;
                // 还没降为0
                if (dropToZeroTime > _fsm.CurrentStateTime)
                {
                    // Debug.Log("松开按键降到0前");
                    // return _initialVelocity - _releaseJumpButtonTime * _pressedAcceleration - _releasedAcceleration * _fsm.CurrentStateTime;
                    return _releasedAcceleration*(dropToZeroTime - _fsm.CurrentStateTime);
                }
                // 已经低于0
                // Debug.Log("松开按键降到0后");
                return Mathf.Max(-_maxDropVelocity, -_gravityAcceleration * (_fsm.CurrentStateTime - dropToZeroTime));
            }
            // 长按时间超过最长时间,视作松开
            if (_releaseJumpButtonTime <= 0 && _fsm.CurrentStateTime >= _longPressMaxTime)
            {
                _releaseJumpButtonTime = _longPressMaxTime;
            }
            // 长按过程中
            // Debug.Log("长按跳跃中");
            return _initialVelocity-_pressedAcceleration*_fsm.CurrentStateTime;
        }
        private void RegisterEvents()
        {
            _fsm.GetData<Variable<bool>>("OnGround").Observe(OnGroundChange);
            // GameEntry.GetModule<EventManager>().Subscribe(InputEventArgs<ButtonState>.GetHashCode("JumpInput"),JumpInputHandler);
            _fsm.GetData<Variable<ButtonState>>("JumpInput").Observe(OnJumpInputChange);
            _fsm.GetData<Variable<ButtonState>>("FireInput").Observe(OnFireInputChange);
            _fsm.GetData<Variable<ButtonState>>("DashInput").Observe(OnDashInputChange);
        }
        
        private void UnRegisterEvents()
        {
            _fsm.GetData<Variable<bool>>("OnGround").Unobserve(OnGroundChange);
            // GameEntry.GetModule<EventManager>().Unsubscribe(InputEventArgs<ButtonState>.GetHashCode("JumpInput"), JumpInputHandler);
            _fsm.GetData<Variable<ButtonState>>("JumpInput").Unobserve(OnJumpInputChange);
            _fsm.GetData<Variable<ButtonState>>("FireInput").Unobserve(OnFireInputChange);
            _fsm.GetData<Variable<ButtonState>>("DashInput").Unobserve(OnDashInputChange);
        }
        private void OnDashInputChange(ButtonState dashInput)
        {
            if(dashInput != ButtonState.Down) return;
            if(!_fsm.Owner.Countdown.HasCountdownCompleted("Dash")) return;
            _fsm.ChangeState<PlayerDashState>();
        }

        
        private void OnFireInputChange(ButtonState fireInput)
        {
            if (fireInput is ButtonState.Down or ButtonState.Up)
            {
                _fsm.Owner.PhysicsWeaponTrigger(fireInput);
            }
        }
        // private void JumpInputHandler(object sender, GameEventArgs e)
        // {
        //     if(e is not InputEventArgs<ButtonState> args) return;
        //     _jumpInput.SetValue(args.Args);
        //     if (args.Args == ButtonState.Up&&_releaseJumpButtonTime<=0)
        //     {
        //         // 松开按键时，如果在短按时间内就视作短按按满，否则就记录释放时间
        //         _releaseJumpButtonTime=Mathf.Max(_fsm.CurrentStateTime,_shortPressJustifyTime);
        //     }
        // }
        private void OnJumpInputChange(ButtonState jumpInput)
        {
            if (jumpInput == ButtonState.Up&&_releaseJumpButtonTime<=0)
            {
                // 松开按键时，如果在短按时间内就视作短按按满，否则就记录释放时间
                _releaseJumpButtonTime=Mathf.Max(_fsm.CurrentStateTime,_shortPressJustifyTime);
            }
            else if (jumpInput == ButtonState.Down)
            {
                if (_jumpToThisState)
                {
                    _lastJumpInputTime=_fsm.CurrentStateTime;
                }
                else if (_fsm.CurrentStateTime <= _coyoteTime)
                {
                    // Debug.Log("土狼时间判定成功");
                    _jumpToThisState = true;
                }
            }
        }
        private void OnGroundChange(bool onGround)
        {
            if(!onGround) return;
            if (_lastJumpInputTime > 0 && _lastJumpInputTime + _jumpInputBufferTime >= _fsm.CurrentStateTime)
            {
                Debug.Log("预输入判定成功");
                _jumpPreInput.SetValue(true);
                _fsm.ChangeState<PlayerJumpState>();
            }
            else
            {
                _fsm.ChangeState<PlayerMoveState>();
            }
            
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
            
            // float verticalVelocity=GetVerticalVelocityStrategyDefault();
            float verticalVelocity = GetVerticalVelocity();
            // Debug.Log(verticalVelocity);
            _velocity.PostValue(_speed*direction+verticalVelocity*Vector3.up);
        }
    }
}