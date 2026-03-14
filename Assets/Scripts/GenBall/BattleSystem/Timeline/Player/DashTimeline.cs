using System.Linq;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Player.Input;
using UnityEngine;

namespace GenBall.BattleSystem.Timeline.Player
{
    public class DashTimeline : TimelineSegmentObj
    {
        private CharacterState _player;
        private InputHandler _input;
        private float _speed;
        private Vector3 _direction;
        private int _movePriority;
        public override void Start()
        {
            if (Timeline.Target == null)
            {
                Debug.LogError("gzp Target不能为null");
                return;
            }

            Timeline.Target.TryGetComponent(out _player);
            if (_player == null)
            {
                Debug.LogError("gzp 未在Target上面找到CharacterState组件");
                return;
            }

            _input = Timeline.Target.GetComponentInChildren<InputHandler>();
            if (_input == null)
            {
                Debug.LogError("gzp 未在Target上找到InputHandler");
                return;
            }
            _direction=_input.MoveDirection;
            var speed = Model.parameters.FirstOrDefault(p => p.Key == "Speed");
            _speed = speed.FloatValue;
            var movePriority = Model.parameters.FirstOrDefault(p => p.Key == "MovePriority");
            _movePriority = movePriority.IntValue;
        }

        public override void Tick(float deltaTime)
        {
            _player.HandleCommand(new MoveCommand(_speed*_direction,_movePriority));
        }

        public override void Clear()
        {
            base.Clear();
            _player = null;
            _input = null;
            _speed = 0;
            _direction = Vector3.zero;
            _movePriority = 0;
        }
    }
}