using System.Collections;
using System.Collections.Generic;
using GenBall.Accessory;
using GenBall.Player;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public class PlayerInitState : PlayerStateBase
    {
        private Variable<bool> _onGround;
        private Fsm<Player> _fsm;
        protected internal override void OnEnter(Fsm<Player> fsm)
        {
            _fsm = fsm;
            _onGround=fsm.GetData<Variable<bool>>("OnGround");
            
            PlayerController.Instance.Init();
            AccessoryController.Instance.Init();
        }

        protected internal override void OnUpdate(Fsm<Player> fsm, float elapsedTime, float realElapseTime)
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
}

